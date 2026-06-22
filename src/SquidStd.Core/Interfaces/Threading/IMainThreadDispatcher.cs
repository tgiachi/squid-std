namespace SquidStd.Core.Interfaces.Threading;

/// <summary>
/// Queues callbacks for execution on the caller that drains the queue.
/// </summary>
public interface IMainThreadDispatcher
{
    /// <summary>
    /// Gets the number of callbacks waiting to be drained.
    /// </summary>
    int PendingCount { get; }

    /// <summary>
    /// Executes queued callbacks on the calling thread.
    /// </summary>
    /// <param name="budgetMs">Optional wall-clock budget in milliseconds.</param>
    /// <returns>The number of callbacks executed.</returns>
    int DrainPending(double? budgetMs = null);

    /// <summary>
    /// Queues a callback for later execution.
    /// </summary>
    /// <param name="action">Callback to execute when the queue is drained.</param>
    void Post(Action action);
}
