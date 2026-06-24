using SquidStd.Mail.Abstractions.Data;
using SquidStd.Mail.Queue.Data.Config;
using SquidStd.Mail.Queue.Interfaces;
using SquidStd.Messaging.Abstractions.Interfaces;

namespace SquidStd.Mail.Queue.Services;

/// <summary>Default <see cref="IMailQueue" />: publishes the message onto the messaging queue.</summary>
public sealed class MailQueue : IMailQueue
{
    private readonly IMessageQueue _queue;
    private readonly string _queueName;

    public MailQueue(IMessageQueue queue, MailQueueOptions options)
    {
        _queue = queue;
        _queueName = string.IsNullOrWhiteSpace(options.QueueName) ? "squidstd.mail.outbound" : options.QueueName;
    }

    /// <inheritdoc />
    public Task EnqueueAsync(OutgoingMailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        return _queue.PublishAsync(_queueName, message, cancellationToken);
    }
}
