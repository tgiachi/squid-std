using SquidStd.Mail.Abstractions.Data;

namespace SquidStd.Mail.Abstractions.Interfaces;

/// <summary>Sends outbound email.</summary>
public interface IMailSender
{
    /// <summary>Sends a message; throws <see cref="Exceptions.MailSendException" /> on failure.</summary>
    Task SendAsync(OutgoingMailMessage message, CancellationToken cancellationToken = default);
}
