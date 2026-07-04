<h1 align="center">SquidStd.Core</h1>

Foundational contracts and utilities for the SquidStd stack. It defines the core service interfaces
(configuration, event bus, jobs, timing, metrics, storage) and ships dependency-free helpers - YAML/JSON
serialization, a Serilog event sink, string/environment/directory extensions, pooled buffers
(`STArrayPool`, `ValueStringBuilder`), and ordinal/case-insensitive string helpers - that the other
SquidStd packages build on.

## Install

```bash
dotnet add package SquidStd.Core
```

## Usage

```csharp
using SquidStd.Core.Extensions.Env;
using SquidStd.Core.Yaml;

// Expand "$VAR" tokens against the environment (unknown vars are left untouched).
var path = "$HOME/squidstd/data".ReplaceEnv();

// Serialize / deserialize YAML.
var yaml = YamlUtils.Serialize(new { name = "squid", port = 9000 });
```

```csharp
using SquidStd.Core.Files;
using SquidStd.Core.Data.Files;
using SquidStd.Core.Pool;

// Watch several directories (each with its own glob) and react via the event bus.
// Via DI: container.RegisterFileWatcherService();  then resolve IFileWatcherService.
var watcher = new FileWatcherService(eventBus);
watcher.Watch("data/scripts", "*.lua");
watcher.Watch("data/templates", "*.json");
eventBus.Subscribe<FileChangedEvent>((change, _) =>
{
    Console.WriteLine($"{change.Kind}: {change.FullPath}");
    return Task.CompletedTask;
});

// Reuse short-lived buffers instead of allocating per call.
using var pool = new ObjectPool<StringBuilder>(() => new StringBuilder(), onReturn: sb => sb.Clear());
var builder = pool.Get();
// ... use builder ...
pool.Return(builder);
```

```csharp
using SquidStd.Core.Extensions.Strings;

"hello world".StartsWithOrdinal("hello"); // true, no culture lookup
"Hello".InsensitiveEquals("hELLO");       // true
```

## Key types

| Type                           | Purpose                                                        |
|--------------------------------|----------------------------------------------------------------|
| `IConfigEntry`                 | A registrable YAML configuration section.                      |
| `IConfigManagerService`        | Loads YAML config and exposes typed sections.                  |
| `IEventBus`                    | Publish/subscribe in-process event bus.                        |
| `IJobSystem`                   | Background job scheduling/execution.                           |
| `ITimerService`                | Timer-wheel based scheduling.                                  |
| `IMetricProvider`              | Source of metric samples for collection.                       |
| `IStorageService`              | File/object storage abstraction.                               |
| `IFileWatcherService`          | Recursive, debounced file watcher publishing to the event bus. |
| `ObjectPool<T>`                | Thread-safe, non-blocking object pool.                         |
| `ICommandDispatcher<TContext>` | Typed protocol command dispatch with context.                  |

## Related

- Tutorial: [Events, jobs & scheduling](https://tgiachi.github.io/squid-std/tutorials/events-jobs-scheduling.html)
- Concept: [Buffers and pooled strings](https://tgiachi.github.io/squid-std/articles/concepts/buffers.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
