namespace SquidStd.Core.Data.Commands;

/// <summary>
/// A handler that threw while processing a dispatched command.
/// </summary>
/// <param name="HandlerType">The concrete handler type that failed.</param>
/// <param name="Exception">The exception thrown by the handler.</param>
public sealed record CommandHandlerError(Type HandlerType, Exception Exception);
