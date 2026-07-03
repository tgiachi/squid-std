using SquidStd.Core.Interfaces.Events;

namespace SquidStd.Core.Data.Bootstrap;

/// <summary>
/// Published on the event bus when the bootstrap has stopped its services.
/// </summary>
/// <param name="Application">The resolved application name.</param>
public sealed record EngineStoppedEvent(string Application) : IEvent;
