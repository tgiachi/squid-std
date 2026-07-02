<h1 align="center">SquidStd.Messaging.RabbitMq</h1>

RabbitMQ transport for SquidStd.Messaging. Implements `IQueueProvider` on top of the RabbitMQ client,
so the same `IMessageQueue` API publishes to and consumes from a real broker. Registered with a single
`AddRabbitMqMessaging(...)` call.

## Install

```bash
dotnet add package SquidStd.Messaging.RabbitMq
```

## Usage

```csharp
using DryIoc;
using SquidStd.Messaging.Abstractions.Interfaces;
using SquidStd.Messaging.RabbitMq.Extensions;

var container = new Container();
container.AddRabbitMqMessaging("rabbitmq://guest:guest@localhost:5672/");

var queue = container.Resolve<IMessageQueue>();
await queue.PublishAsync("orders", new { Id = 1 });
```

## Key types

| Type                                      | Purpose                                   |
|-------------------------------------------|-------------------------------------------|
| `RabbitMqMessagingRegistrationExtensions` | `AddRabbitMqMessaging(...)` registration. |
| `RabbitMqQueueProvider`                   | RabbitMQ-backed `IQueueProvider`.         |
| `RabbitMqOptions`                         | Connection + prefetch configuration.      |

## Related

- Tutorial: [Messaging](https://tgiachi.github.io/squid-std/tutorials/messaging.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
