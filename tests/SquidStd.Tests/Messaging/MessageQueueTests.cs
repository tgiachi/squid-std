using SquidStd.Messaging;
using SquidStd.Messaging.Abstractions;
using SquidStd.Messaging.Abstractions.Interfaces;

namespace SquidStd.Tests.Messaging;

public class MessageQueueTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    private sealed class Order
    {
        public string Id { get; set; } = "";
        public int Amount { get; set; }
    }

    private sealed class CapturingListener : IQueueMessageListenerAsync<Order>
    {
        public TaskCompletionSource<Order> Received { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task HandleAsync(Order message, CancellationToken cancellationToken)
        {
            Received.TrySetResult(message);

            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task PublishAsync_DeliversTypedMessageToListener()
    {
        await using var provider = new InMemoryQueueProvider(new MessagingOptions(), new MessagingMetricsProvider());
        IMessageQueue queue = new MessageQueue(provider, new JsonMessageSerializer());
        var listener = new CapturingListener();
        queue.Subscribe("orders", listener);

        await queue.PublishAsync("orders", new Order { Id = "A1", Amount = 42 });

        var received = await listener.Received.Task.WaitAsync(Timeout);
        Assert.Equal("A1", received.Id);
        Assert.Equal(42, received.Amount);
    }
}
