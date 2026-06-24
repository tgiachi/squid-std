using SquidStd.Core.Interfaces.Events;

namespace SquidStd.Mail.Abstractions.Data.Events;

/// <summary>Published on the event bus for each newly received email.</summary>
/// <param name="Message">The received message.</param>
public sealed record MailReceivedEvent(MailMessage Message) : IEvent;
