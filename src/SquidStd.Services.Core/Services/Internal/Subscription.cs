namespace SquidStd.Services.Core.Services.Internal;

/// <summary>
///     Unsubscribe token that removes a listener from its bucket when disposed.
/// </summary>
internal sealed class Subscription : IDisposable
{
    private readonly List<object> _bucket;
    private readonly object _listener;
    private bool _disposed;

    public Subscription(List<object> bucket, object listener)
    {
        _bucket = bucket;
        _listener = listener;
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
            _bucket.Remove(_listener);
        }
    }
}
