using SquidStd.Mail.Abstractions.Data;
using SquidStd.Mail.Abstractions.Interfaces;

namespace SquidStd.Tests.Mail.Support;

/// <summary>Test <see cref="IMailSender" /> that records the messages it was asked to send.</summary>
public sealed class RecordingMailSender : IMailSender
{
    private readonly List<OutgoingMailMessage> _sent = [];
    private readonly Lock _sync = new();

    public IReadOnlyList<OutgoingMailMessage> Sent
    {
        get
        {
            lock (_sync)
            {
                return _sent.ToArray();
            }
        }
    }

    public Task SendAsync(OutgoingMailMessage message, CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            _sent.Add(message);
        }

        return Task.CompletedTask;
    }
}
