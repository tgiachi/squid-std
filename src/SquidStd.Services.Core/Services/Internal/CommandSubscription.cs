namespace SquidStd.Services.Core.Services.Internal;

/// <summary>
///     Unsubscribe token that removes a command handler from its bucket when disposed.
/// </summary>
internal sealed class CommandSubscription : IDisposable
{
    private readonly List<object> _bucket;
    private readonly object _handler;
    private bool _disposed;

    public CommandSubscription(List<object> bucket, object handler)
    {
        _bucket = bucket;
        _handler = handler;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        lock (_bucket)
        {
            _bucket.Remove(_handler);
        }
    }
}
