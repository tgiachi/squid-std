using SquidStd.Core.Interfaces.Threading;

namespace SquidStd.Services.Core.Services.Internal;

/// <summary>
/// Synchronization context that forwards asynchronous posts to a main-thread dispatcher.
/// </summary>
internal sealed class MainThreadSynchronizationContext : SynchronizationContext
{
    private readonly IMainThreadDispatcher _dispatcher;

    public MainThreadSynchronizationContext(IMainThreadDispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        _dispatcher = dispatcher;
    }

    /// <inheritdoc />
    public override void Post(SendOrPostCallback d, object? state)
    {
        ArgumentNullException.ThrowIfNull(d);
        _dispatcher.Post(() => d(state));
    }

    /// <inheritdoc />
    public override void Send(SendOrPostCallback d, object? state)
    {
        ArgumentNullException.ThrowIfNull(d);
        d(state);
    }
}
