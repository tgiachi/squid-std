# Configuration

SquidStd loads configuration from a single YAML file. Each module registers a
strongly-typed section; the config manager deserializes it, expands environment
variables, and publishes the populated object into the container so you can
resolve it like any other service.

## Steps

1. **Point the bootstrap at your config file.** `SquidStdOptions.ConfigName` is the
   logical file name (default `squidstd`, so `squidstd.yaml`) and
   `RootDirectory` is the directory it is searched in.
2. **Register a section.** Call `RegisterConfigSection<TConfig>` with the section
   name (the top-level YAML key). Provide a `createDefault` factory so the file
   is generated with sensible defaults on first run.
3. **Use environment variables.** Any `string` property whose value contains a
   `$VAR` token is expanded from the environment when the section is loaded.
   Unknown tokens are left untouched.
4. **Read values.** Resolve the config type from the container wherever you need it.

```csharp
public sealed class MyServiceConfig
{
    public string Endpoint { get; set; } = string.Empty; // e.g. "https://$API_HOST"
    public int Retries { get; set; } = 3;
}

var bootstrap = SquidStdBootstrap.Create(
    new SquidStdOptions { ConfigName = "squidstd", RootDirectory = AppContext.BaseDirectory });

bootstrap.ConfigureServices(container =>
    container.RegisterConfigSection("myService", static () => new MyServiceConfig()));

await bootstrap.StartAsync();

var config = container.Resolve<MyServiceConfig>();
```

The corresponding `squidstd.yaml`:

```yaml
myService:
  endpoint: "https://$API_HOST"
  retries: 5
```

## Inspecting and overriding loaded configuration

The config manager loads sections transparently, but nothing stays hidden:

```csharp
var config = bootstrap.Resolve<IConfigManagerService>();
Console.WriteLine(config.Compose());                      // full current configuration as YAML
var logger = config.GetConfig<SquidStdLoggerOptions>();   // one typed section
```

To inspect or tweak a section at startup - before the logger and the services consume it -
register a typed hook on the bootstrap. Hooks run after every configuration load and mutate
the section in memory only: the YAML file is never rewritten (call `Save()` explicitly if you
want to persist).

```csharp
// Tune a section at startup
bootstrap.OnConfigLoaded<SquidStdLoggerOptions>(o => o.MinimumLevel = LogLevelType.Debug);

// Override from the environment (WorkersConfig comes with SquidStd.Workers)
bootstrap.OnConfigLoaded<WorkersConfig>(w =>
{
    if (int.TryParse(Environment.GetEnvironmentVariable("WORKERS_MAX"), out var max))
    {
        w.MaxConcurrency = max;
    }
});

// Inspect a loaded section before services start
bootstrap.OnConfigLoaded<StorageConfig>(s => Console.WriteLine($"storage root: {s.RootDirectory}"));
```

Hooks compose: register as many as you need, also on the same section - they run in
registration order. Registering a hook for a type that is not a config section fails at
startup with a clear error; registering one after the bootstrap has started throws.

To receive the whole configuration in one callback - after every typed hook has been
applied - use `OnConfigReady`. It hands you the config manager, so you can dump or inspect
the final values:

```csharp
bootstrap.OnConfigReady(cfg =>
{
    Console.WriteLine(cfg.Compose());   // final configuration, overrides included
});
```

`OnConfigReady` follows the same rules as the typed hooks: it runs after every configuration
load, before the logger and the services, and must be registered before the bootstrap starts.
