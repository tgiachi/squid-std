---
_layout: landing
---

<div class="sqd-hero">
  <img src="images/logo.png" alt="SquidStd" width="120" height="120" />
  <h1>SquidStd</h1>
  <p class="sqd-tagline">A batteries-included, modular standard library for .NET 10 - distilled from years
  of building real-world server software. Each capability ships behind a small contract with an in-memory
  implementation for tests and a production backend.</p>
  <p class="sqd-badges">
    <a href="https://www.nuget.org/packages/SquidStd.Core"><img src="https://img.shields.io/nuget/v/SquidStd.Core?label=nuget&color=1390A3" alt="NuGet" /></a>
    <a href="https://github.com/tgiachi/squid-std/actions/workflows/ci.yml"><img src="https://img.shields.io/github/actions/workflow/status/tgiachi/squid-std/ci.yml?branch=main&label=CI" alt="CI" /></a>
    <a href="https://github.com/tgiachi/squid-std/blob/main/LICENSE"><img src="https://img.shields.io/badge/license-MIT-4BB4BD" alt="MIT" /></a>
  </p>
  <pre class="sqd-install"><code>dotnet add package SquidStd.Services.Core</code></pre>
  <p class="sqd-cta">
    <a class="btn btn-primary" href="tutorials/getting-started.md">Get started</a>
    <a class="btn btn-outline-primary" href="api/index.md">API reference</a>
  </p>
</div>

## Capabilities

<div class="sqd-cards">
  <a class="sqd-card" href="articles/messaging-abstractions.md"><b>Messaging</b><span>Queues &amp; pub/sub - in-memory, RabbitMQ, SQS</span></a>
  <a class="sqd-card" href="articles/caching-abstractions.md"><b>Caching</b><span>In-memory or Redis behind one contract</span></a>
  <a class="sqd-card" href="articles/storage-abstractions.md"><b>Storage</b><span>Files &amp; objects - local or S3/MinIO</span></a>
  <a class="sqd-card" href="articles/vfs-abstractions.md"><b>Virtual FS</b><span>Composable filesystems with decorators</span></a>
  <a class="sqd-card" href="articles/database-abstractions.md"><b>Database</b><span>FreeSql data access with migrations</span></a>
  <a class="sqd-card" href="articles/crypto.md"><b>Crypto</b><span>PGP, password ciphers, encrypted vaults</span></a>
  <a class="sqd-card" href="articles/mail-abstractions.md"><b>Mail</b><span>Send, receive and queue email</span></a>
  <a class="sqd-card" href="articles/workers-abstractions.md"><b>Workers</b><span>Job handlers, heartbeats, a manager API</span></a>
  <a class="sqd-card" href="articles/actors.md"><b>Actors</b><span>Mailbox-based concurrency primitives</span></a>
  <a class="sqd-card" href="articles/network.md"><b>Network</b><span>TCP/UDP servers with framing pipelines</span></a>
  <a class="sqd-card" href="articles/templating.md"><b>Templating</b><span>Scriban templates with includes</span></a>
  <a class="sqd-card" href="articles/telemetry-abstractions.md"><b>Telemetry</b><span>OpenTelemetry tracing &amp; metrics</span></a>
</div>

## Quick start

```csharp
var container = new Container();
container.AddInMemoryMessaging();

var bootstrap = SquidStdBootstrap.Create(new SquidStdOptions { ConfigName = "myapp" }, container);
await bootstrap.StartAsync();
```

Continue with the [getting started tutorial](tutorials/getting-started.md).

## Explore

- **[Tutorials](tutorials/index.md)** - learn by building: bootstrap, caching, messaging, workers, crypto, persistence, and more.
- **[Guides](articles/guides/configuration.md)** - task-focused how-to and "which provider" decision guides.
- **[Concepts](articles/concepts/architecture.md)** - the architecture and the ideas behind it.
- **[Packages](articles/getting-started.md)** - per-package reference.
- **[Felix Network](articles/felix.md)** - companion secure binary mesh-networking library (.NET + C/ESP32).
