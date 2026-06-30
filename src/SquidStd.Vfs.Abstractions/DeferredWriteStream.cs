namespace SquidStd.Vfs.Abstractions;

/// <summary>
/// A writable in-memory stream that flushes its accumulated bytes through a callback exactly once on
/// disposal. Backs <c>OpenWriteAsync</c> for filesystems that persist the whole payload at close time.
/// </summary>
public sealed class DeferredWriteStream : MemoryStream
{
    private readonly Func<byte[], CancellationToken, ValueTask> _onFlush;
    private readonly CancellationToken _cancellationToken;
    private bool _flushed;

    public DeferredWriteStream(Func<byte[], CancellationToken, ValueTask> onFlush, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(onFlush);

        _onFlush = onFlush;
        _cancellationToken = cancellationToken;
    }

    public override async ValueTask DisposeAsync()
    {
        if (!_flushed)
        {
            _flushed = true;
            await _onFlush(ToArray(), _cancellationToken).ConfigureAwait(false);
        }

        await base.DisposeAsync().ConfigureAwait(false);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_flushed)
        {
            _flushed = true;
            _onFlush(ToArray(), _cancellationToken).AsTask().GetAwaiter().GetResult();
        }

        base.Dispose(disposing);
    }
}
