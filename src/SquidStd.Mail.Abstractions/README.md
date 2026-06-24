<p align="center">
  <img src="https://raw.githubusercontent.com/tgiachi/squid-std/main/assets/icon.png" alt="SquidStd" width="120" height="120" />
</p>

<h1 align="center">SquidStd.Mail.Abstractions</h1>

<p align="center">
  <a href="https://www.nuget.org/packages/SquidStd.Mail.Abstractions/"><img src="https://img.shields.io/nuget/v/SquidStd.Mail.Abstractions.svg" alt="NuGet" /></a>
  <img src="https://img.shields.io/nuget/dt/SquidStd.Mail.Abstractions.svg" alt="Downloads" />
  <a href="https://tgiachi.github.io/squid-std/articles/mail-abstractions.html"><img src="https://img.shields.io/badge/docs-DocFX-1390A3.svg" alt="docs" /></a>
  <img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="license" />
</p>

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

## License

MIT — part of [SquidStd](https://github.com/tgiachi/squid-std).
