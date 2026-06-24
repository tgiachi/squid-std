<p align="center">
  <img src="https://raw.githubusercontent.com/tgiachi/squid-std/main/assets/icon.png" alt="SquidStd" width="120" height="120" />
</p>

<h1 align="center">SquidStd.Mail.Queue</h1>

<p align="center">
  <a href="https://www.nuget.org/packages/SquidStd.Mail.Queue/"><img src="https://img.shields.io/nuget/v/SquidStd.Mail.Queue.svg" alt="NuGet" /></a>
  <img src="https://img.shields.io/nuget/dt/SquidStd.Mail.Queue.svg" alt="Downloads" />
  <a href="https://tgiachi.github.io/squid-std/articles/mail-queue.html"><img src="https://img.shields.io/badge/docs-DocFX-1390A3.svg" alt="docs" /></a>
  <img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="license" />
</p>

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

## License

MIT — part of [SquidStd](https://github.com/tgiachi/squid-std).
