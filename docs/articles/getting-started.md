# Getting Started

Install the package(s) you need and bootstrap the core services.

```bash
dotnet add package SquidStd.Services.Core
```

```csharp
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Services.Core.Services.Bootstrap;

// Create registers the configuration core: DirectoriesConfig, the `logger`
// section and the config manager. Everything else is explicit.
var bootstrap = SquidStdBootstrap.Create(
    new SquidStdOptions { ConfigName = "squidstd", RootDirectory = AppContext.BaseDirectory });

// Opt into the core services: JSON serializer, event bus, job system,
// main-thread dispatcher, timer wheel, metrics and secrets.
bootstrap.ConfigureServices(container => container.RegisterCoreServices());

await bootstrap.StartAsync();
// … resolve services …
await bootstrap.StopAsync();
```

Opt into modules with `ConfigureServices`, e.g. `container.AddInMemoryCache()`. See the
[Concepts](concepts/bootstrap-lifecycle.md) for the full lifecycle and
[Packages](getting-started.md) for per-module reference.
