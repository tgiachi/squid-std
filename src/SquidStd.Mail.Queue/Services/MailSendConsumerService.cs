using Serilog;
using SquidStd.Abstractions.Interfaces.Services;
using SquidStd.Mail.Abstractions.Data;
using SquidStd.Mail.Abstractions.Interfaces;
using SquidStd.Mail.Queue.Data.Config;
using SquidStd.Messaging.Abstractions.Interfaces;

namespace SquidStd.Mail.Queue.Services;

/// <summary>
/// Consumes queued outbound messages and sends them via <see cref="IMailSender" />. Exceptions propagate so the
/// messaging layer retries / dead-letters.
/// </summary>
public sealed class MailSendConsumerService : ISquidStdService, IQueueMessageListenerAsync<OutgoingMailMessage>
{
    private readonly ILogger _logger = Log.ForContext<MailSendConsumerService>();
    private readonly IMessageQueue _queue;
    private readonly string _queueName;
    private readonly IMailSender _sender;
    private IDisposable? _subscription;

    public MailSendConsumerService(IMessageQueue queue, IMailSender sender, MailQueueOptions options)
    {
        _queue = queue;
        _sender = sender;
        _queueName = string.IsNullOrWhiteSpace(options.QueueName) ? "squidstd.mail.outbound" : options.QueueName;
    }

    /// <inheritdoc />
    public Task HandleAsync(OutgoingMailMessage message, CancellationToken cancellationToken)
        => _sender.SendAsync(message, cancellationToken);

    /// <inheritdoc />
    public ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        _subscription = _queue.Subscribe(_queueName, this);
        _logger.Information("Mail send consumer listening on queue '{Queue}'.", _queueName);

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        _subscription?.Dispose();
        _subscription = null;

        return ValueTask.CompletedTask;
    }
}
