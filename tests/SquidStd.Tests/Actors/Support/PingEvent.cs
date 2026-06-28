using SquidStd.Core.Interfaces.Events;

namespace SquidStd.Tests.Actors.Support;

/// <summary>Event used to verify the EventBus-to-mailbox adapter.</summary>
public sealed record PingEvent(string Text) : IEvent;
