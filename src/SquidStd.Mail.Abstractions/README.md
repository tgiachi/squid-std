<h1 align="center">SquidStd.Mail.Abstractions</h1>

Mail contracts for SquidStd: the `MailMessage` model, the `MailReceivedEvent`, the `IMailReader` interface, and
the plain `MailOptions`.

## Install

```bash
dotnet add package SquidStd.Mail.Abstractions
```

## Key types

| Type                                    | Purpose                                                         |
|-----------------------------------------|-----------------------------------------------------------------|
| `MailMessage`                           | Parsed email (headers, bodies, attachments, optional raw .eml). |
| `MailReceivedEvent`                     | Published on the event bus per new message.                     |
| `IMailReader`                           | Fetches new messages from a mailbox.                            |
| `MailOptions`                           | Host/port/credentials, protocol, polling and fetch options.     |
| `IMailSender`                           | Sends outbound email.                                           |
| `OutgoingMailMessage`                   | An email to send (recipients, subject, bodies, attachments).    |
| `SmtpOptions`                           | SMTP host/port/SSL/credentials and a default sender.            |
| `MailSentEvent` / `MailSendFailedEvent` | Published on the event bus on send success/failure.             |

## Related

- Tutorial: [Email](https://tgiachi.github.io/squid-std/tutorials/email.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
