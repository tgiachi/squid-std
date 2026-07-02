<h1 align="center">SquidStd.Messaging.RabbitMq</h1>

RabbitMQ transport for SquidStd.Messaging. Implements `IQueueProvider` and `ITopicProvider` on top of the
RabbitMQ client, so the same `IMessageQueue` / `IMessageTopic` API that runs in-memory during tests
publishes to and consumes from a real broker in production - registered with a single
`AddRabbitMqMessaging(...)` call.

The provider declares durable **quorum queues** and publishes persistent messages, so queued work survives
broker restarts. Each queue gets a companion dead-letter queue (`<queue>.dlq` by default), wired via
`x-delivery-limit` so the broker itself moves a message there after `MaxDeliveryAttempts` failed
deliveries. Connections are created with automatic recovery enabled, and consumers apply the configured
prefetch via basic QoS. Topics map to fan-out exchanges with an exclusive, auto-delete queue per
subscriber - topic delivery is live fan-out, not durable.

## Install

```bash
dotnet add package SquidStd.Messaging.RabbitMq
```

## Usage

Register through the bootstrap - the providers are `ISquidStdService`s, so `bootstrap.StartAsync()` opens
the broker connection and `StopAsync` closes it:

```csharp
using SquidStd.Messaging.Abstractions.Interfaces;
using SquidStd.Messaging.RabbitMq.Data.Config;
using SquidStd.Messaging.RabbitMq.Extensions;
using SquidStd.Services.Core.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;

var bootstrap = SquidStdBootstrap.Create(o => o.ConfigName = "myapp");

bootstrap.ConfigureServices(c => c.RegisterCoreServices().AddRabbitMqMessaging(new RabbitMqOptions
{
    HostName = "rabbit.internal",
    UserName = "app",
    Password = "secret"
}));

await bootstrap.StartAsync();
```

Publishing and consuming are identical to the in-memory provider - the contracts come from
`SquidStd.Messaging.Abstractions`:

```csharp
var queue = bootstrap.Resolve<IMessageQueue>();

using (queue.Subscribe("orders", new OrderListener()))   // IQueueMessageListenerAsync<OrderPlaced>
{
    await queue.PublishAsync("orders", new OrderPlaced("order-1"));
}

var topic = bootstrap.Resolve<IMessageTopic>();
using var sub = topic.Subscribe<OrderPlaced>("order-events", (order, _) =>
{
    Console.WriteLine($"topic saw {order.Id}");
    return Task.CompletedTask;
});
```

## Configuration

`AddRabbitMqMessaging(RabbitMqOptions options, MessagingOptions? messagingOptions = null)` takes the
connection settings plus the shared messaging options (delivery attempts, dead-letter suffix). Options are
code-only - this package registers no YAML config section.

| `RabbitMqOptions` property | Default       | Purpose                                             |
|----------------------------|---------------|-----------------------------------------------------|
| `HostName`                 | `"localhost"` | Broker host.                                        |
| `Port`                     | `5672`        | Broker port.                                        |
| `VirtualHost`              | `"/"`         | Virtual host.                                       |
| `UserName`                 | `"guest"`     | User name.                                          |
| `Password`                 | `"guest"`     | Password.                                           |
| `Uri`                      | `null`        | AMQP URI; when set it overrides the fields above.   |
| `PrefetchCount`            | `10`          | Consumer prefetch (basic QoS) per subscriber.       |

A connection-string overload is also available:
`AddRabbitMqMessaging("rabbitmq://app:secret@rabbit.internal:5672/myvhost?prefetch=20&maxDeliveryAttempts=5")`.

`MessagingMetricsProvider` is registered as `IMessagingMetrics` / `IMetricProvider`; the provider reports
published, delivered, and failed counts plus subscriber counts per queue.

## Key types

| Type                                      | Purpose                                                  |
|-------------------------------------------|----------------------------------------------------------|
| `RabbitMqMessagingRegistrationExtensions` | `AddRabbitMqMessaging(...)` registration.                |
| `RabbitMqQueueProvider`                   | Quorum-queue `IQueueProvider` with broker-side DLQ.      |
| `RabbitMqTopicProvider`                   | Fan-out exchange `ITopicProvider`.                       |
| `RabbitMqOptions`                         | Connection + prefetch configuration.                     |

## Related

- Article: [Messaging with RabbitMQ](https://tgiachi.github.io/squid-std/articles/messaging-rabbitmq.html)
- Article: [Messaging](https://tgiachi.github.io/squid-std/articles/messaging.html)
- Tutorial: [Messaging](https://tgiachi.github.io/squid-std/tutorials/messaging.html)
- Siblings: [SquidStd.Messaging (in-memory)](https://tgiachi.github.io/squid-std/articles/messaging.html), [SquidStd.Messaging.Sqs](https://tgiachi.github.io/squid-std/articles/messaging-sqs.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
