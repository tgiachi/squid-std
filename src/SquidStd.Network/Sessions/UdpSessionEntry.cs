namespace SquidStd.Network.Sessions;

/// <summary>
///     Internal registry entry pairing a UDP session with its last-activity timestamp.
/// </summary>
/// <typeparam name="TState">Application-defined per-connection state.</typeparam>
internal sealed class UdpSessionEntry<TState>
{
    public UdpSessionEntry(Session<TState> session, DateTimeOffset lastActivityUtc)
    {
        Session = session;
        LastActivityUtc = lastActivityUtc;
    }

    public Session<TState> Session { get; }
    public DateTimeOffset LastActivityUtc { get; set; }
}
