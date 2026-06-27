using SquidStd.Actors;

namespace SquidStd.Tests.Actors.Support;

/// <summary>Marker interface for every message the <see cref="ProbeActor" /> accepts.</summary>
public interface IProbeMessage
{
}

/// <summary>Appends a value to the actor's log.</summary>
public sealed record Append(string Value) : IProbeMessage;

/// <summary>Causes the handler to throw.</summary>
public sealed record Boom : IProbeMessage;

/// <summary>Blocks the consumer until <paramref name="Gate" /> completes (ignores cancellation).</summary>
public sealed record Hold(TaskCompletionSource Gate) : IProbeMessage;

/// <summary>Blocks the consumer until the actor is cancelled (honors the token).</summary>
public sealed record HoldUntilCancelled : IProbeMessage;

/// <summary>Request that replies with the comma-joined log.</summary>
public sealed record GetLog : ActorRequest<string>, IProbeMessage;

/// <summary>Request whose handler throws, to exercise Ask exception propagation.</summary>
public sealed record FailingRequest : ActorRequest<string>, IProbeMessage;
