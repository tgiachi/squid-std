using SquidStd.Actors.Types;

namespace SquidStd.Actors.Data;

/// <summary>
///     Configuration for an <see cref="SquidStd.Actors.Actor{TMessage}" /> mailbox.
/// </summary>
public sealed class ActorOptions
{
    /// <summary>Bounded mailbox capacity (ignored when <see cref="OverflowPolicy" /> is Unbounded).</summary>
    public int Capacity { get; init; } = 1024;

    /// <summary>Behavior when the mailbox is full.</summary>
    public ActorOverflowPolicy OverflowPolicy { get; init; } = ActorOverflowPolicy.Wait;

    /// <summary>Behavior when a fire-and-forget handler throws.</summary>
    public ActorErrorPolicy ErrorPolicy { get; init; } = ActorErrorPolicy.Isolate;
}
