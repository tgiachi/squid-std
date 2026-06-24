using SquidStd.Mail.Abstractions.Data;

namespace SquidStd.Mail.Queue.Interfaces;

/// <summary>Enqueues outbound email for asynchronous sending.</summary>
public interface IMailQueue
{
    /// <summary>Enqueues a message; it is sent later by the background consumer.</summary>
    Task EnqueueAsync(OutgoingMailMessage message, CancellationToken cancellationToken = default);
}
