using System.Diagnostics;
using Serilog;
using SquidStd.Abstractions.Interfaces.Services;
using SquidStd.Core.Data.EventLoop;
using SquidStd.Core.Data.Metrics;
using SquidStd.Core.Interfaces.Metrics;
using SquidStd.Core.Interfaces.Threading;
using SquidStd.Core.Interfaces.Timing;
using SquidStd.Core.Types.Metrics;

namespace SquidStd.Services.Core.Services.EventLoop;

/// <summary>
/// General-purpose event loop. Owns a dedicated background thread that each frame drains the
/// <see cref="IMainThreadDispatcher" /> and advances the timer wheel (<see cref="ITimerService" />),
/// sleeps when idle, and exposes tick metrics. Mutually exclusive with the timer-wheel pump.
/// </summary>
public sealed class EventLoopService : IEventLoopService, ISquidStdService, IMetricProvider, ITimerWheelDriver, IDisposable
{
    private readonly IMainThreadDispatcher _dispatcher;
    private readonly ITimerService _timer;
    private readonly EventLoopConfig _config;
    private readonly CancellationTokenSource _cts = new();
    private readonly ILogger _logger = Log.ForContext<EventLoopService>();
    private readonly Lock _metricsSync = new();
    private double _averageTickMs;
    private long _idleSleepCount;
    private double _maxTickMs;
    private Thread? _thread;
    private long _tickCount;
    private volatile int _loopThreadId = -1;

    /// <inheritdoc />
    public long TickCount => Interlocked.Read(ref _tickCount);

    /// <inheritdoc />
    public double AverageTickMs
    {
        get
        {
            lock (_metricsSync)
            {
                return _averageTickMs;
            }
        }
    }

    /// <inheritdoc />
    public double MaxTickMs
    {
        get
        {
            lock (_metricsSync)
            {
                return _maxTickMs;
            }
        }
    }

    /// <inheritdoc />
    public bool IsOnLoopThread => _loopThreadId == Environment.CurrentManagedThreadId;

    /// <inheritdoc />
    public string ProviderName => "eventloop";

    public EventLoopService(IMainThreadDispatcher dispatcher, ITimerService timer, EventLoopConfig config)
    {
        _dispatcher = dispatcher;
        _timer = timer;
        _config = config;
    }

    /// <inheritdoc />
    public ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        _thread = new Thread(RunLoop)
        {
            IsBackground = true,
            Name = "SquidStd-EventLoop"
        };
        _thread.Start();

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        _cts.Cancel();
        _thread?.Join(TimeSpan.FromSeconds(5));

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask<IReadOnlyList<MetricSample>> CollectAsync(CancellationToken cancellationToken = default)
    {
        double avg;
        double max;

        lock (_metricsSync)
        {
            avg = _averageTickMs;
            max = _maxTickMs;
        }

        IReadOnlyList<MetricSample> samples =
        [
            new("tick_count", Interlocked.Read(ref _tickCount), Type: MetricType.Counter, Help: "Total event loop iterations"),
            new("tick_avg_ms", avg, Help: "EMA tick elapsed in ms"),
            new("tick_max_ms", max, Help: "Worst tick elapsed in ms"),
            new("idle_sleeps_total", Interlocked.Read(ref _idleSleepCount), Type: MetricType.Counter, Help: "Total idle sleeps")
        ];

        return new ValueTask<IReadOnlyList<MetricSample>>(samples);
    }

    internal int Tick()
    {
        var start = Stopwatch.GetTimestamp();
        var work = _dispatcher.DrainPending();
        var nowMs = (long)Math.Floor(Stopwatch.GetTimestamp() * 1000.0 / Stopwatch.Frequency);
        work += _timer.UpdateTicksDelta(nowMs);
        var elapsed = Stopwatch.GetElapsedTime(start);

        UpdateMetrics(elapsed);

        if (elapsed.TotalMilliseconds >= _config.SlowTickThresholdMs)
        {
            _logger.Warning(
                "Slow tick: {Elapsed:0.###}ms work={Work} pending={Pending}",
                elapsed.TotalMilliseconds,
                work,
                _dispatcher.PendingCount
            );
        }

        return work;
    }

    private void RunLoop()
    {
        _loopThreadId = Environment.CurrentManagedThreadId;

        while (!_cts.IsCancellationRequested)
        {
            var work = Tick();

            if (_config.IdleCpuEnabled && work == 0)
            {
                Thread.Sleep(_config.IdleSleepMs);
                Interlocked.Increment(ref _idleSleepCount);
            }
        }
    }

    private void UpdateMetrics(TimeSpan elapsed)
    {
        Interlocked.Increment(ref _tickCount);

        lock (_metricsSync)
        {
            // Exponential moving average: 0.95 weight to history, 0.05 to current sample.
            _averageTickMs = _averageTickMs * 0.95 + elapsed.TotalMilliseconds * 0.05;
            _maxTickMs = Math.Max(_maxTickMs, elapsed.TotalMilliseconds);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }
}
