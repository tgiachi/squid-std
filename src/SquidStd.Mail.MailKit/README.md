<h1 align="center">SquidStd.Mail.MailKit</h1>

MailKit-backed IMAP/POP3 provider for SquidStd.Mail. Polls a mailbox on the timer wheel and publishes a
`MailReceivedEvent` on the `IEventBus` for each new message.

## Install

```bash
dotnet add package SquidStd.Mail.MailKit
```

## Usage

```csharp
using DryIoc;
using SquidStd.Mail.Abstractions.Data.Config;
using SquidStd.Mail.Abstractions.Types.Mail;
using SquidStd.Mail.MailKit.Extensions;

container.AddMail(new MailOptions
{
    Protocol = MailProtocolType.Imap,
    Host = "imap.example.com",
    Port = 993,
    UseSsl = true,
    Username = "user@example.com",
    Password = "secret",
    PollIntervalSeconds = 30,
    IncludeRawEml = true
});
```

Listen with an `IAsyncEventListener<MailReceivedEvent>` registered on the `IEventBus`. IMAP marks messages
`\Seen` after fetch (configurable); POP3 dedups by UIDL and can delete after download.

## Sending (SMTP)

```csharp
using DryIoc;
using SquidStd.Mail.Abstractions.Data;
using SquidStd.Mail.Abstractions.Data.Config;
using SquidStd.Mail.Abstractions.Interfaces;
using SquidStd.Mail.MailKit.Extensions;

container.AddMailSender(new SmtpOptions
{
    Host = "smtp.example.com",
    Port = 587,
    UseSsl = false,
    Username = "user@example.com",
    Password = "secret",
    DefaultFrom = new MailAddress("App", "app@example.com")
});

var sender = container.Resolve<IMailSender>();
await sender.SendAsync(new OutgoingMailMessage
{
    To = [new MailAddress("Bob", "bob@example.com")],
    Subject = "Welcome",
    HtmlBody = "<p>Hello!</p>"
});
```

`MailSentEvent` / `MailSendFailedEvent` are published on the `IEventBus`; failures throw `MailSendException`.

## Key types

| Type | Purpose |
|------|---------|
| `MailRegistrationExtensions` | `AddMail(...)` registration (IMAP/POP3 polling). |
| `MailSenderRegistrationExtensions` | `AddMailSender(...)` registration (SMTP). |
| `ImapMailReader` / `Pop3MailReader` | `IMailReader` implementations. |
| `MailKitMailSender` | `IMailSender` implementation over SMTP. |

## Related

- Tutorial: [Email](https://tgiachi.github.io/squid-std/tutorials/email.html)

## License

MIT — part of [SquidStd](https://github.com/tgiachi/squid-std).
