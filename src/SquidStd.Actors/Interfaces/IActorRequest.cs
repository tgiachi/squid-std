namespace SquidStd.Actors.Interfaces;

/// <summary>
///     A request message that an actor answers with a single typed reply.
/// </summary>
/// <typeparam name="TReply">The reply type.</typeparam>
public interface IActorRequest<TReply> : IActorRequestCore
{
    /// <summary>The task the caller of <c>AskAsync</c> awaits.</summary>
    Task<TReply> Completion { get; }

    /// <summary>Completes the request with a reply.</summary>
    /// <param name="value">The reply value.</param>
    void Reply(TReply value);
}
