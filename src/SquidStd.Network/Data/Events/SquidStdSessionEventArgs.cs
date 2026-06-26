using SquidStd.Network.Sessions;

namespace SquidStd.Network.Data.Events;

/// <summary>
///     Event payload carrying a session (created or removed).
/// </summary>
public sealed class SquidStdSessionEventArgs<TState> : EventArgs
{
    public SquidStdSessionEventArgs(Session<TState> session)
    {
        Session = session;
    }

    /// <summary>The session associated with the event.</summary>
    public Session<TState> Session { get; }
}
