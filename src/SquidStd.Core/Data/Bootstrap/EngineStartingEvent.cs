using SquidStd.Core.Interfaces.Events;

namespace SquidStd.Core.Data.Bootstrap;

/// <summary>
/// Published on the event bus when the bootstrap begins starting services.
/// </summary>
/// <param name="Application">The resolved application name.</param>
public sealed record EngineStartingEvent(string Application) : IEvent;
