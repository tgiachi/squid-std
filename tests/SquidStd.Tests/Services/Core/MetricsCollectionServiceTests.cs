using SquidStd.Core.Data.Metrics;
using SquidStd.Core.Interfaces.Events;
using SquidStd.Core.Interfaces.Metrics;
using SquidStd.Services.Core.Services;

namespace SquidStd.Tests.Services.Core;

public class MetricsCollectionServiceTests
{
    [Fact]
    public void GetSnapshot_WhenNotStarted_ReturnsEmptySnapshot()
    {
        using var service = new MetricsCollectionService([], new());

        var snapshot = service.GetSnapshot();

        Assert.Equal(DateTimeOffset.MinValue, snapshot.CollectedAt);
        Assert.Empty(snapshot.Metrics);
    }

    [Fact]
    public async Task GetStatus_ReturnsLatestMetricsSnapshot()
    {
        using var service = new MetricsCollectionService(
            [new CountingMetricProvider("jobs", "completed.total", 11)],
            new()
            {
                IntervalMilliseconds = 10,
                LogEnabled = false
            }
        );
        IMetricsCollectionService metrics = service;

        await service.StartAsync(CancellationToken.None);
        await WaitUntilAsync(() => metrics.GetStatus().Metrics.ContainsKey("jobs.completed.total"));

        var status = metrics.GetStatus();

        Assert.NotEqual(DateTimeOffset.MinValue, status.CollectedAt);
        Assert.Equal(11, status.Metrics["jobs.completed.total"].Value);
    }

    [Fact]
    public async Task StartAsync_CollectsMetricsFromRegisteredProviders()
    {
        using var service = new MetricsCollectionService(
            [new CountingMetricProvider("jobs", "pending.total", 7)],
            new()
            {
                IntervalMilliseconds = 10,
                LogEnabled = false
            }
        );

        await service.StartAsync(CancellationToken.None);
        await WaitUntilAsync(() => service.GetAllMetrics().ContainsKey("jobs.pending.total"));

        var metrics = service.GetAllMetrics();
        var sample = metrics["jobs.pending.total"];

        Assert.Equal("pending.total", sample.Name);
        Assert.Equal(7, sample.Value);
        Assert.NotNull(sample.Timestamp);
    }

    [Fact]
    public async Task StartAsync_PublishesMetricsCollectedEventThroughEventBus()
    {
        using var eventBus = new EventBusService();
        IEventBus bus = eventBus;
        var firstListener = new MetricsCollectedListener();
        var secondListener = new MetricsCollectedListener();
        bus.RegisterListener(firstListener);
        bus.RegisterListener(secondListener);
        using var service = new MetricsCollectionService(
            [new CountingMetricProvider("events", "published.total", 5)],
            new()
            {
                IntervalMilliseconds = 1000,
                LogEnabled = false
            },
            bus
        );

        await service.StartAsync(CancellationToken.None);

        // The collection loop's first tick still has to be scheduled via Task.Run, so give it
        // more headroom than the default: on a loaded CI runner that scheduling can lag well
        // past a couple of seconds even though the collector itself collects immediately.
        await WaitUntilAsync(
            () => firstListener.LastEvent is not null && secondListener.LastEvent is not null,
            TimeSpan.FromSeconds(30)
        );
        await service.StopAsync(CancellationToken.None);

        Assert.NotNull(firstListener.LastEvent);
        Assert.NotNull(secondListener.LastEvent);
        Assert.Same(firstListener.LastEvent, secondListener.LastEvent);
        Assert.Same(service.GetStatus(), firstListener.LastEvent.Snapshot);
        Assert.Equal(5, firstListener.LastEvent.Snapshot.Metrics["events.published.total"].Value);
    }

    [Fact]
    public async Task StartAsync_WhenDisabled_DoesNotCollectMetrics()
    {
        var provider = new CountingMetricProvider("jobs", "pending.total", 7);
        using var service = new MetricsCollectionService(
            [provider],
            new()
            {
                Enabled = false,
                IntervalMilliseconds = 10,
                LogEnabled = false
            }
        );

        await service.StartAsync(CancellationToken.None);
        await Task.Delay(30);

        Assert.Empty(service.GetAllMetrics());
        Assert.Equal(0, provider.CollectionCount);
    }

    [Fact]
    public async Task StartAsync_WhenProviderThrows_ContinuesCollectingOtherProviders()
    {
        using var service = new MetricsCollectionService(
            [
                new ThrowingMetricProvider("broken"),
                new CountingMetricProvider("timer", "callbacks.total", 3)
            ],
            new()
            {
                IntervalMilliseconds = 10,
                LogEnabled = false
            }
        );

        await service.StartAsync(CancellationToken.None);
        await WaitUntilAsync(() => service.GetAllMetrics().ContainsKey("timer.callbacks.total"));

        var metrics = service.GetAllMetrics();

        Assert.Contains("timer.callbacks.total", metrics.Keys);
        Assert.DoesNotContain("broken", metrics.Keys);
    }

    [Fact]
    public async Task StopAsync_StopsCollectionLoop()
    {
        var provider = new CountingMetricProvider("bus", "dispatch.total", 1);
        using var service = new MetricsCollectionService(
            [provider],
            new()
            {
                IntervalMilliseconds = 10,
                LogEnabled = false
            }
        );

        await service.StartAsync(CancellationToken.None);
        await WaitUntilAsync(() => provider.CollectionCount > 0);
        await service.StopAsync(CancellationToken.None);
        var countAfterStop = provider.CollectionCount;

        await Task.Delay(50);

        Assert.Equal(countAfterStop, provider.CollectionCount);
    }

    private static async Task WaitUntilAsync(Func<bool> predicate, TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(2));

        while (DateTime.UtcNow < deadline)
        {
            if (predicate())
            {
                return;
            }

            await Task.Delay(10);
        }

        Assert.Fail("Condition was not met before timeout.");
    }

    private sealed class CountingMetricProvider : IMetricProvider
    {
        private readonly string _metricName;
        private readonly double _value;
        private int _collectionCount;

        public int CollectionCount => Volatile.Read(ref _collectionCount);

        public string ProviderName { get; }

        public CountingMetricProvider(string providerName, string metricName, double value)
        {
            ProviderName = providerName;
            _metricName = metricName;
            _value = value;
        }

        public ValueTask<IReadOnlyList<MetricSample>> CollectAsync(CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _collectionCount);

            return ValueTask.FromResult<IReadOnlyList<MetricSample>>([new(_metricName, _value)]);
        }
    }

    private sealed class ThrowingMetricProvider : IMetricProvider
    {
        public string ProviderName { get; }

        public ThrowingMetricProvider(string providerName)
        {
            ProviderName = providerName;
        }

        public ValueTask<IReadOnlyList<MetricSample>> CollectAsync(CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Synthetic test failure.");
    }

    private sealed class MetricsCollectedListener : IEventListener<MetricsCollectedEvent>
    {
        public MetricsCollectedEvent? LastEvent { get; private set; }

        public Task HandleAsync(MetricsCollectedEvent eventData, CancellationToken cancellationToken = default)
        {
            LastEvent = eventData;

            return Task.CompletedTask;
        }
    }
}
