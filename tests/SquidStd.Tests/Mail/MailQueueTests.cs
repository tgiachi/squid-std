using DryIoc;
using SquidStd.Core.Interfaces.Events;
using SquidStd.Mail.Abstractions.Data;
using SquidStd.Mail.Queue.Data.Config;
using SquidStd.Mail.Queue.Services;
using SquidStd.Messaging.Abstractions.Interfaces;
using SquidStd.Messaging.Extensions;
using SquidStd.Services.Core.Services;

namespace SquidStd.Tests.Mail;

public class MailQueueTests
{
    private sealed class CapturingListener : IQueueMessageListenerAsync<OutgoingMailMessage>
    {
        private readonly TaskCompletionSource<OutgoingMailMessage> _completion;

        public CapturingListener(TaskCompletionSource<OutgoingMailMessage> completion)
        {
            _completion = completion;
        }

        public Task HandleAsync(OutgoingMailMessage message, CancellationToken cancellationToken)
        {
            _completion.TrySetResult(message);

            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task EnqueueAsync_PublishesMessageToConfiguredQueue()
    {
        var container = new Container();
        container.RegisterInstance<IEventBus>(new EventBusService());
        container.AddInMemoryMessaging();
        var messageQueue = container.Resolve<IMessageQueue>();

        var options = new MailQueueOptions();
        var received = new TaskCompletionSource<OutgoingMailMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var _ = messageQueue.Subscribe(options.QueueName, new CapturingListener(received));

        var queue = new MailQueue(messageQueue, options);
        await queue.EnqueueAsync(
            new()
            {
                To = [new("Bob", "bob@example.com")],
                Subject = "queued"
            }
        );

        var message = await received.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal("queued", message.Subject);
        Assert.Contains(message.To, a => a.Address == "bob@example.com");
    }
}
