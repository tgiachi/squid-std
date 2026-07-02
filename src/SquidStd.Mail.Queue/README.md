<h1 align="center">SquidStd.Mail.Queue</h1>

Outbound mail send queue for SquidStd. Enqueue an `OutgoingMailMessage`; a background consumer sends it via
`IMailSender`. Retry, backoff, and dead-lettering come from the SquidStd messaging queue.

## Install

```bash
dotnet add package SquidStd.Mail.Queue
```

## Usage

```csharp
using DryIoc;
using SquidStd.Messaging.Extensions;
using SquidStd.Mail.Abstractions.Data;
using SquidStd.Mail.MailKit.Extensions;
using SquidStd.Mail.Queue.Extensions;
using SquidStd.Mail.Queue.Interfaces;

container.AddInMemoryMessaging();           // or AddRabbitMqMessaging(...)
container.AddMailSender(new SmtpOptions { Host = "smtp.example.com", Port = 587 });
container.AddMailQueue();

var queue = container.Resolve<IMailQueue>();
await queue.EnqueueAsync(new OutgoingMailMessage
{
    To = [new MailAddress("Bob", "bob@example.com")],
    Subject = "Welcome",
    HtmlBody = "<p>Hello!</p>"
});
```

Retry/backoff/dead-letter are configured via `MessagingOptions` (`MaxDeliveryAttempts`, `RetryDelay`,
`DeadLetterQueueSuffix`). With RabbitMQ the queue is durable across restarts.

## Key types

| Type                              | Purpose                                                           |
|-----------------------------------|-------------------------------------------------------------------|
| `IMailQueue`                      | Enqueue an `OutgoingMailMessage` for background delivery.         |
| `MailQueue`                       | `IMailQueue` implementation over the SquidStd messaging queue.    |
| `MailSendConsumerService`         | Background consumer that sends queued messages via `IMailSender`. |
| `MailQueueRegistrationExtensions` | `AddMailQueue(...)` registration.                                 |
| `MailQueueOptions`                | Queue name and send options.                                      |

## Related

- Tutorial: [Email](https://tgiachi.github.io/squid-std/tutorials/email.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
