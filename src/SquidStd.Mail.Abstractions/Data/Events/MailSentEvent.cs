using SquidStd.Core.Interfaces.Events;
using SquidStd.Mail.Abstractions.Data;

namespace SquidStd.Mail.Abstractions.Data.Events;

/// <summary>Published after a message is sent successfully.</summary>
/// <param name="To">Primary recipients.</param>
/// <param name="Subject">Message subject.</param>
public sealed record MailSentEvent(IReadOnlyList<MailAddress> To, string Subject) : IEvent;
