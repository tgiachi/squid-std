namespace SquidStd.Actors.Types;

/// <summary>
/// Behavior when the actor mailbox reaches its bounded capacity.
/// </summary>
public enum ActorOverflowPolicy
{
    /// <summary>Await until capacity frees (back-pressure). Default.</summary>
    Wait,

    /// <summary>Drop the newest message; the send returns <c>false</c>.</summary>
    DropNewest,

    /// <summary>No bound; never blocks and never drops.</summary>
    Unbounded
}
