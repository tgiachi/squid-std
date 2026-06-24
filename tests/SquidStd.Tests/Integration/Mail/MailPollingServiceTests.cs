using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SquidStd.Core.Interfaces.Events;
using SquidStd.Services.Core.Services;
using SquidStd.Tests.Mail.Support;
using SquidStd.Mail.Abstractions.Data.Config;
using SquidStd.Mail.Abstractions.Data.Events;
using SquidStd.Mail.Abstractions.Types.Mail;
using SquidStd.Mail.MailKit.Services;

namespace SquidStd.Tests.Integration.Mail;

[Collection(GreenMailCollection.Name)]
public class MailPollingServiceTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);

    private readonly GreenMailContainerFixture _fixture;

    public MailPollingServiceTests(GreenMailContainerFixture fixture)
    {
        _fixture = fixture;
    }

    private sealed class DelegateListener : IAsyncEventListener<MailReceivedEvent>
    {
        private readonly Action<MailReceivedEvent> _onEvent;

        public DelegateListener(Action<MailReceivedEvent> onEvent)
        {
            _onEvent = onEvent;
        }

        public Task HandleAsync(MailReceivedEvent eventData, CancellationToken cancellationToken)
        {
            _onEvent(eventData);

            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task PollOnce_PublishesMailReceivedEvent()
    {
        var user = "poll-user@example.com";
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Sender", "sender@example.com"));
        message.To.Add(new MailboxAddress("Rcpt", user));
        message.Subject = "poll-subject";
        message.Body = new TextPart("plain") { Text = "hi" };

        using (var smtp = new SmtpClient())
        {
            await smtp.ConnectAsync(_fixture.Host, _fixture.SmtpPort, SecureSocketOptions.None).WaitAsync(Timeout);
            await smtp.SendAsync(message).WaitAsync(Timeout);
            await smtp.DisconnectAsync(true).WaitAsync(Timeout);
        }

        var eventBus = new EventBusService();
        var received = new TaskCompletionSource<MailReceivedEvent>(TaskCreationOptions.RunContinuationsAsynchronously);
        eventBus.RegisterAsyncListener(new DelegateListener(e => received.TrySetResult(e)));

        var reader = new ImapMailReader(new MailOptions
        {
            Protocol = MailProtocolType.Imap,
            Host = _fixture.Host,
            Port = _fixture.ImapPort,
            UseSsl = false,
            Username = user,
            Password = "pwd"
        });

        using var service = new MailPollingService(reader, eventBus, new FakeTimerService(), new MailOptions { PollIntervalSeconds = 60 });
        await service.PollOnceAsync();

        var evt = await received.Task.WaitAsync(Timeout);
        Assert.Equal("poll-subject", evt.Message.Subject);
    }
}
