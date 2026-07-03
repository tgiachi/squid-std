using SquidStd.Core.Interfaces.Events;

namespace SquidStd.Core.Data.Bootstrap;

/// <summary>
/// Published on the event bus when every service has started.
/// </summary>
/// <param name="Application">The resolved application name.</param>
/// <param name="ServiceCount">Number of services started.</param>
/// <param name="ElapsedMs">Total startup time in milliseconds.</param>
public sealed record EngineStartedEvent(string Application, int ServiceCount, double ElapsedMs) : IEvent;
