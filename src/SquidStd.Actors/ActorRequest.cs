using SquidStd.Actors.Interfaces;

namespace SquidStd.Actors;

/// <summary>
/// Base request record that removes the <see cref="TaskCompletionSource{TResult}" /> boilerplate.
/// Derive from it together with your actor's message interface, e.g.
/// <c>record GetNick : ActorRequest&lt;string?&gt;, ISessionMessage;</c>.
/// </summary>
/// <typeparam name="TReply">The reply type.</typeparam>
public abstract record ActorRequest<TReply> : IActorRequest<TReply>
{
    private readonly TaskCompletionSource<TReply> _completion =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <inheritdoc />
    public Task<TReply> Completion => _completion.Task;

    /// <inheritdoc />
    public void Reply(TReply value)
        => _completion.TrySetResult(value);

    /// <inheritdoc />
    public void Fail(Exception error)
        => _completion.TrySetException(error);
}
