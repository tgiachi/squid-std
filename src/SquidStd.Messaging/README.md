<h1 align="center">SquidStd.Messaging</h1>

In-memory transport for SquidStd.Messaging. Provides channel-backed `IQueueProvider` and `ITopicProvider`
implementations behind the shared `IMessageQueue` / `IMessageTopic` facades: per-queue buffering,
round-robin (competing-consumers) delivery, retry and dead-letter handling, topic fan-out, and metrics -
all registered with a single `AddInMemoryMessaging()` call.

Queues deliver each message to exactly one subscriber; topics fan every message out to all subscribers.
The contracts live in `SquidStd.Messaging.Abstractions`, so code written against `IMessageQueue` and
`IMessageTopic` moves unchanged to a real broker: use this in-memory provider for single-process apps,
tests, and local dev, then swap in `SquidStd.Messaging.RabbitMq` or `SquidStd.Messaging.Sqs` for
production.

## Install

```bash
dotnet add package SquidStd.Messaging
```

## Usage

Register through the bootstrap - the providers are `ISquidStdService`s, so `StartAsync` / `StopAsync`
manage their lifecycle for you:

```csharp
using SquidStd.Messaging.Abstractions.Interfaces;
using SquidStd.Messaging.Extensions;
using SquidStd.Services.Core.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;

var bootstrap = SquidStdBootstrap.Create(o => o.ConfigName = "myapp");
bootstrap.ConfigureServices(c => c.RegisterCoreServices().AddInMemoryMessaging());
await bootstrap.StartAsync();
```

### Queues (competing consumers)

Each queue message goes to exactly one subscriber. Implement a listener and subscribe it; dispose the
subscription to unsubscribe.

```csharp
public sealed record OrderPlaced(string Id);

public sealed class OrderListener : IQueueMessageListenerAsync<OrderPlaced>
{
    public Task HandleAsync(OrderPlaced message, CancellationToken cancellationToken)
    {
        Console.WriteLine($"queue handled {message.Id}");
        return Task.CompletedTask;
    }
}

var queue = bootstrap.Resolve<IMessageQueue>();

using (queue.Subscribe("orders", new OrderListener()))
{
    await queue.PublishAsync("orders", new OrderPlaced("order-1"));
}
```

A synchronous `IQueueMessageListener<TMessage>` overload of `Subscribe` is also available.

### Topics (fan-out)

Every subscriber receives every message published to the topic.

```csharp
var topic = bootstrap.Resolve<IMessageTopic>();

using (topic.Subscribe<OrderPlaced>("order-events", (order, _) =>
{
    Console.WriteLine($"topic saw {order.Id}");
    return Task.CompletedTask;
}))
{
    await topic.PublishAsync("order-events", new OrderPlaced("order-2"));
}
```

## Configuration

`AddInMemoryMessaging` takes an optional `MessagingOptions`. Options are code-only - this package
registers no YAML config section.

```csharp
c.AddInMemoryMessaging(new MessagingOptions
{
    MaxDeliveryAttempts = 5,
    DeadLetterQueueSuffix = ".dead",
    RetryDelay = TimeSpan.FromMilliseconds(250)
});
```

| Property                | Default | Purpose                                                     |
|-------------------------|---------|-------------------------------------------------------------|
| `MaxDeliveryAttempts`   | `3`     | Delivery attempts before a message is dead-lettered.        |
| `DeadLetterQueueSuffix` | `".dlq"`| Suffix appended to a queue name to form its dead-letter queue. |
| `RetryDelay`            | `Zero`  | Delay before a failed message is re-enqueued.               |

A connection-string overload is also available:
`AddInMemoryMessaging("memory://local?maxDeliveryAttempts=5&retryDelayMs=250&deadLetterSuffix=.dead")`.

When a listener throws, the message is re-enqueued (after `RetryDelay`) until `MaxDeliveryAttempts` is
reached, then moved to `<queue><DeadLetterQueueSuffix>` - subscribe to that queue to inspect failures.
`MessagingMetricsProvider` is registered as `IMessagingMetrics` / `IMetricProvider` and tracks published,
delivered, retried, failed, and dead-lettered counts per queue.

## Key types

| Type                              | Purpose                                                    |
|-----------------------------------|------------------------------------------------------------|
| `MessagingRegistrationExtensions` | `AddInMemoryMessaging(...)` registration.                  |
| `InMemoryQueueProvider`           | Channel-based in-memory `IQueueProvider` with retry/DLQ.   |
| `InMemoryTopicProvider`           | In-memory `ITopicProvider` (fan-out).                      |
| `IMessageQueue` / `IMessageTopic` | Typed facades from `SquidStd.Messaging.Abstractions`.      |
| `MessagingOptions`                | Retry, dead-letter, and delivery-attempt configuration.    |

## Related

- Article: [Messaging](https://tgiachi.github.io/squid-std/articles/messaging.html)
- Article: [Messaging abstractions](https://tgiachi.github.io/squid-std/articles/messaging-abstractions.html)
- Tutorial: [Messaging](https://tgiachi.github.io/squid-std/tutorials/messaging.html)
- Production transports: [SquidStd.Messaging.RabbitMq](https://tgiachi.github.io/squid-std/articles/messaging-rabbitmq.html), [SquidStd.Messaging.Sqs](https://tgiachi.github.io/squid-std/articles/messaging-sqs.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
