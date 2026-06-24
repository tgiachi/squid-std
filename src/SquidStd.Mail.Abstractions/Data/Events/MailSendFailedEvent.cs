using SquidStd.Core.Interfaces.Events;
using SquidStd.Mail.Abstractions.Data;

namespace SquidStd.Mail.Abstractions.Data.Events;

/// <summary>Published when sending a message fails.</summary>
/// <param name="To">Primary recipients.</param>
/// <param name="Subject">Message subject.</param>
/// <param name="Error">Failure reason.</param>
public sealed record MailSendFailedEvent(IReadOnlyList<MailAddress> To, string Subject, string Error) : IEvent;
