namespace SquidStd.Core.Interfaces.Commands;

/// <summary>
///     Handles a command of type <typeparamref name="TCommand" /> within the ambient
///     <typeparamref name="TContext" /> (for example the current session or connection).
/// </summary>
/// <typeparam name="TCommand">The command type handled.</typeparam>
/// <typeparam name="TContext">The ambient context type passed to the handler.</typeparam>
public interface ICommandHandler<in TCommand, in TContext>
    where TCommand : ICommand
{
    /// <summary>Handles a dispatched command.</summary>
    /// <param name="command">The command payload.</param>
    /// <param name="context">The ambient context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when handling finishes.</returns>
    Task HandleAsync(TCommand command, TContext context, CancellationToken cancellationToken = default);
}
