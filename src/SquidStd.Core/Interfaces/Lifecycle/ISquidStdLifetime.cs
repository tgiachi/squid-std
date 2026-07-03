namespace SquidStd.Core.Interfaces.Lifecycle;

/// <summary>
/// Tracks shutdown requests for the host and exposes a shared cancellation token.
/// </summary>
public interface ISquidStdLifetime
{
    /// <summary>Token cancelled when a shutdown has been requested.</summary>
    CancellationToken ShutdownToken { get; }

    /// <summary>Requests a graceful shutdown. Idempotent.</summary>
    void RequestShutdown();
}
