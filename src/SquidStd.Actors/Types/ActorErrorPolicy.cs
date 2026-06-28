namespace SquidStd.Actors.Types;

/// <summary>
///     Behavior when a handler throws while processing a fire-and-forget message.
/// </summary>
public enum ActorErrorPolicy
{
    /// <summary>Log and continue; the actor stays alive. Default.</summary>
    Isolate,

    /// <summary>Stop the actor; the mailbox faults and pending requests fail.</summary>
    StopOnError
}
