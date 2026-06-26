using SquidStd.Messaging.Abstractions.Interfaces;

namespace SquidStd.Tests.Workers.Support;

/// <summary>Inert <see cref="IMessageQueue" /> for unit tests that drive the consumer's HandleAsync directly.</summary>
public sealed class FakeMessageQueue : IMessageQueue
{
    public Task PublishAsync<TMessage>(string queueName, TMessage message, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public IDisposable Subscribe<TMessage>(string queueName, IQueueMessageListener<TMessage> listener)
    {
        return new Subscription();
    }

    public IDisposable Subscribe<TMessage>(string queueName, IQueueMessageListenerAsync<TMessage> listener)
    {
        return new Subscription();
    }

    private sealed class Subscription : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
