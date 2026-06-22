using System.Collections.Concurrent;
using System.Diagnostics;
using Serilog;
using SquidStd.Core.Interfaces.Threading;

namespace SquidStd.Services.Core.Services;

/// <summary>
/// Queues callbacks and drains them on the calling thread.
/// </summary>
public sealed class MainThreadDispatcherService : IMainThreadDispatcher
{
    private readonly ILogger _logger = Log.ForContext<MainThreadDispatcherService>();
    private readonly ConcurrentQueue<Action> _queue = new();

    /// <inheritdoc />
    public int PendingCount => _queue.Count;

    /// <inheritdoc />
    public int DrainPending(double? budgetMs = null)
    {
        var snapshot = _queue.Count;

        if (snapshot == 0)
        {
            return 0;
        }

        var stopwatch = budgetMs is null ? null : Stopwatch.StartNew();
        var executed = 0;

        while (executed < snapshot && _queue.TryDequeue(out var action))
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "MainThreadDispatcherService callback failed");
            }

            executed++;

            if (stopwatch is not null && stopwatch.Elapsed.TotalMilliseconds >= budgetMs!.Value)
            {
                break;
            }
        }

        return executed;
    }

    /// <inheritdoc />
    public void Post(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _queue.Enqueue(action);
    }
}
