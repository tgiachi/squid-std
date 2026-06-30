using SquidStd.Core.Interfaces.Commands;

namespace SquidStd.Services.Core.Services.Internal;

/// <summary>
/// Adapts a delegate to <see cref="ICommandHandler{TCommand,TContext}" /> so the dispatcher has a
/// single dispatch path.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TContext">The context type.</typeparam>
internal sealed class DelegateCommandHandler<TCommand, TContext> : ICommandHandler<TCommand, TContext>
    where TCommand : ICommand
{
    private readonly Func<TCommand, TContext, CancellationToken, Task> _handler;

    public DelegateCommandHandler(Func<TCommand, TContext, CancellationToken, Task> handler)
    {
        _handler = handler;
    }

    public Task HandleAsync(TCommand command, TContext context, CancellationToken cancellationToken = default)
        => _handler(command, context, cancellationToken);
}
