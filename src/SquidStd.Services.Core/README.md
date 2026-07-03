<h1 align="center">SquidStd.Services.Core</h1>

Concrete implementations of the SquidStd.Core contracts, wired for DryIoc. `RegisterCoreServices()`
brings up the core services - JSON serializer, event bus, job system, timer wheel, main-thread
dispatcher, metrics collection, and secrets. The `RegisterCoreServices(configName, configDirectory)`
overload also registers the configuration core (directories, `logger` section, config manager) for
standalone containers. File storage moved to `SquidStd.Storage` (`AddFileStorage()`), which also
registers the `storage` config section.

## Install

```bash
dotnet add package SquidStd.Services.Core
```

## Usage

```csharp
using DryIoc;
using SquidStd.Services.Core.Extensions;

var container = new Container();

// Standalone container: config core + serializer + event bus + jobs + timer wheel
// + dispatcher + metrics + secrets.
container.RegisterCoreServices("squidstd", Directory.GetCurrentDirectory());
```

With `SquidStdBootstrap`, `Create` already registers the configuration core, so opt into the core
services with the parameterless overload (or pick individual ones with the granular
`RegisterEventBusService()`, `RegisterTimerWheelService()`, … methods):

```csharp
using SquidStd.Services.Core.Services.Bootstrap;

var bootstrap = SquidStdBootstrap.Create(o => o.ConfigName = "squidstd");
bootstrap.ConfigureServices(c => c.RegisterCoreServices());

await bootstrap.StartAsync();
```

Need to inspect or override a loaded config section before services start? Register a typed hook
(in-memory only - the YAML file is untouched):

```csharp
bootstrap.OnConfigLoaded<SquidStdLoggerOptions>(o => o.MinimumLevel = LogLevelType.Debug);
```

### Command dispatch

```csharp
// Register a dispatcher for your context type and handlers.
container.RegisterCommandDispatcher<Session>();
container.RegisterCommandHandler<PingCommand, Session, PingHandler>();

// Dispatch (handlers auto-subscribed at bootstrap). Pass the context explicitly:
var result = await dispatcher.DispatchAsync(new PingCommand("hi"), session);
if (!result.Matched)
{
    // unknown command
}
```

For network-driven dispatch, build the context from a seed (e.g. the originating connection):

```csharp
container.RegisterCommandDispatcher<Session>();
container.RegisterCommandHandler<PingCommand, Session, PingHandler>();
container.RegisterSeededCommandDispatcher<Session, Connection, ConnectionSessionFactory>();

// in the receive loop, where the connection is known:
var seeded = container.Resolve<ISeededCommandDispatcher<Session, Connection>>();
await seeded.DispatchAsync(command, connection);   // factory maps connection -> Session
```

## Key types

| Type                                | Purpose                                                                   |
|-------------------------------------|---------------------------------------------------------------------------|
| `RegisterDefaultServicesExtensions` | `RegisterCoreServices()` / `RegisterConfigManagerService()` entry points. |
| `ConfigManagerService`              | YAML config load/save with env-var substitution.                          |
| `EventBusService`                   | In-process event bus implementation.                                      |
| `CommandDispatcher<TContext>`       | Typed protocol command dispatch with context.                             |
| `JobSystemService`                  | Background job execution.                                                 |
| `TimerWheelService`                 | Timer-wheel scheduling.                                                   |
| `MainThreadDispatcherService`       | Main-thread work dispatch.                                                |
| `MetricsCollectionService`          | Metric sample aggregation.                                                |
| `EventLoopService`                  | Frame-driven loop: drains the dispatcher and advances the timer wheel.    |

## Related

- Tutorial: [Events, jobs & scheduling](https://tgiachi.github.io/squid-std/tutorials/events-jobs-scheduling.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
