namespace SquidStd.Core.Data.Commands;

/// <summary>
///     The outcome of dispatching a command.
/// </summary>
/// <param name="Matched">True when at least one handler was registered for the command type.</param>
/// <param name="HandlerCount">The number of handlers invoked.</param>
/// <param name="Errors">Handlers that threw a non-cancellation exception.</param>
public sealed record CommandDispatchResult(bool Matched, int HandlerCount, IReadOnlyList<CommandHandlerError> Errors);
