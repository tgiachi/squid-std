using System.Text;
using SquidStd.Core.Interfaces.Events;
using SquidStd.Mail.Abstractions.Data.Events;
using SquidStd.Mail.Abstractions.Exceptions;
using SquidStd.Mail.Abstractions.Types.Mail;
using SquidStd.Mail.MailKit.Services;
using SquidStd.Services.Core.Services;

namespace SquidStd.Tests.Integration.Mail;

[Collection(GreenMailCollection.Name)]
public class MailSenderTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);

    private readonly GreenMailContainerFixture _fixture;

    public MailSenderTests(GreenMailContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SendAsync_DeliversMessage_AndPublishesMailSent()
    {
        var recipient = "send-target@example.com";
        var eventBus = new EventBusService();
        var sent = new TaskCompletionSource<MailSentEvent>(TaskCreationOptions.RunContinuationsAsynchronously);
        eventBus.RegisterListener(new DelegateListener<MailSentEvent>(e => sent.TrySetResult(e)));

        var sender = new MailKitMailSender(
            new() { Host = _fixture.Host, Port = _fixture.SmtpPort, UseSsl = false },
            eventBus
        );

        await sender.SendAsync(
                        new()
                        {
                            From = new("Sender", "sender@example.com"),
                            To = [new("Target", recipient)],
                            Subject = "sent-subject",
                            TextBody = "hello",
                            Attachments = [new("a.txt", "text/plain", Encoding.UTF8.GetBytes("xyz"))]
                        }
                    )
                    .WaitAsync(Timeout);

        var sentEvent = await sent.Task.WaitAsync(Timeout);
        Assert.Equal("sent-subject", sentEvent.Subject);

        var reader = new ImapMailReader(
            new()
            {
                Protocol = MailProtocolType.Imap,
                Host = _fixture.Host,
                Port = _fixture.ImapPort,
                UseSsl = false,
                Username = recipient,
                Password = "pwd"
            }
        );

        var received = await reader.FetchNewAsync().WaitAsync(Timeout);
        Assert.Contains(received, m => m.Subject == "sent-subject");
    }

    [Fact]
    public async Task SendAsync_Throws_AndPublishesFailed_OnClosedPort()
    {
        var eventBus = new EventBusService();
        var failed = new TaskCompletionSource<MailSendFailedEvent>(TaskCreationOptions.RunContinuationsAsynchronously);
        eventBus.RegisterListener(new DelegateListener<MailSendFailedEvent>(e => failed.TrySetResult(e)));

        var sender = new MailKitMailSender(new() { Host = _fixture.Host, Port = 1, UseSsl = false }, eventBus);

        await Assert.ThrowsAsync<MailSendException>(
            () => sender.SendAsync(
                            new()
                            {
                                From = new("Sender", "sender@example.com"),
                                To = [new("Target", "x@example.com")],
                                Subject = "fail-subject"
                            }
                        )
                        .WaitAsync(Timeout)
        );

        var failedEvent = await failed.Task.WaitAsync(Timeout);
        Assert.Equal("fail-subject", failedEvent.Subject);
    }

    private sealed class DelegateListener<TEvent> : IEventListener<TEvent>
        where TEvent : IEvent
    {
        private readonly Action<TEvent> _onEvent;

        public DelegateListener(Action<TEvent> onEvent)
        {
            _onEvent = onEvent;
        }

        public Task HandleAsync(TEvent eventData, CancellationToken cancellationToken)
        {
            _onEvent(eventData);

            return Task.CompletedTask;
        }
    }
}
