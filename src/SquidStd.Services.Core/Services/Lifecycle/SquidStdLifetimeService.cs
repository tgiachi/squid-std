using SquidStd.Core.Interfaces.Lifecycle;

namespace SquidStd.Services.Core.Services.Lifecycle;

/// <summary>
/// Default <see cref="ISquidStdLifetime" /> backed by a cancellation token source owned by
/// the bootstrap.
/// </summary>
public sealed class SquidStdLifetimeService : ISquidStdLifetime, IDisposable
{
    private readonly CancellationTokenSource _shutdownSource = new();

    /// <inheritdoc />
    public CancellationToken ShutdownToken => _shutdownSource.Token;

    /// <inheritdoc />
    public void RequestShutdown()
    {
        if (_shutdownSource.IsCancellationRequested)
        {
            return;
        }

        try
        {
            _shutdownSource.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Requested after the bootstrap disposed the lifetime: idempotent no-op.
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _shutdownSource.Dispose();
        GC.SuppressFinalize(this);
    }
}
