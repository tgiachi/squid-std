<h1 align="center">SquidStd.Messaging</h1>

In-memory transport for SquidStd.Messaging. Provides a channel-backed `IQueueProvider` with per-queue
buffering, round-robin delivery to subscribers, retry/dead-letter handling, and metrics - registered
with a single `AddInMemoryMessaging()` call. Ideal for single-process apps, tests, and local dev.

## Install

```bash
dotnet add package SquidStd.Messaging
```

## Usage

```csharp
using DryIoc;
using SquidStd.Messaging.Abstractions.Interfaces;
using SquidStd.Messaging.Extensions;

var container = new Container();
container.AddInMemoryMessaging();

var queue = container.Resolve<IMessageQueue>();
await queue.PublishAsync("orders", new { Id = 1 });
```

## Key types

| Type                              | Purpose                                   |
|-----------------------------------|-------------------------------------------|
| `MessagingRegistrationExtensions` | `AddInMemoryMessaging(...)` registration. |
| `InMemoryQueueProvider`           | Channel-based in-memory `IQueueProvider`. |

## Related

- Tutorial: [Messaging](https://tgiachi.github.io/squid-std/tutorials/messaging.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
