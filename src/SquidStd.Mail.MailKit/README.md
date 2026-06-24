<p align="center">
  <img src="https://raw.githubusercontent.com/tgiachi/squid-std/main/assets/icon.png" alt="SquidStd" width="120" height="120" />
</p>

<h1 align="center">SquidStd.Mail.MailKit</h1>

<p align="center">
  <a href="https://www.nuget.org/packages/SquidStd.Mail.MailKit/"><img src="https://img.shields.io/nuget/v/SquidStd.Mail.MailKit.svg" alt="NuGet" /></a>
  <img src="https://img.shields.io/nuget/dt/SquidStd.Mail.MailKit.svg" alt="Downloads" />
  <a href="https://tgiachi.github.io/squid-std/articles/mail-mailkit.html"><img src="https://img.shields.io/badge/docs-DocFX-1390A3.svg" alt="docs" /></a>
  <img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="license" />
</p>

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

## License

MIT — part of [SquidStd](https://github.com/tgiachi/squid-std).
