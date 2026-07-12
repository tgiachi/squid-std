# Configuration

SquidStd loads configuration from a single YAML file, eagerly, before any service is
registered. Each module binds a strongly-typed section at registration time: the value is
already in the container as soon as the `Register*` call returns, expanded and ready to
resolve like any other service.

## The config-first flow

1. **Create loads eagerly.** `SquidStdBootstrap.Create(options)` reads the YAML file into a
   standalone `SquidStdConfig` before any container registration happens (a missing file
   yields an empty document; nothing is written yet).
2. **Registrations bind immediately.** Every `RegisterConfigSection<TConfig>` call - direct,
   or through a `RegisterXxx` helper - binds its section against that `SquidStdConfig` right
   away and registers the resulting instance as a singleton. There is no "config not ready
   yet" window: resolve the type straight after the `Register*` call.
3. **Hooks run once, at `StartAsync`.** Typed `OnConfigLoaded<T>` hooks and `OnConfigReady`
   callbacks are applied a single time, right before the logger is configured and services
   start. They mutate the already-bound instances in place.
4. **Explicit `Load()` is a reload.** Call `IConfigManagerService.Load()` to re-read the file
   from disk, re-bind every tracked section, and re-apply the hooks. This is the only way
   configuration changes after startup - there is no background file watch.

## Steps

1. **Point the bootstrap at your config file.** `SquidStdOptions.ConfigName` is the
   logical file name (default `squidstd`, so `squidstd.yaml`) and
   `RootDirectory` is the directory it is searched in.
2. **Register a section.** Call `RegisterConfigSection<TConfig>` with the section
   name (the top-level YAML key). Provide a `createDefault` factory so the file
   is generated with sensible defaults on first run (the file is written at `StartAsync`
   when it does not exist yet, once every section is known).
3. **Use environment variables in SquidStd sections.** Any `string` property of a type whose
   namespace starts with `SquidStd` (the framework's own sections and any section type shipped
   in a `SquidStd.*` package) is expanded from the environment when the section is bound: a
   `$VAR` token is replaced with the matching environment variable, unknown tokens are left
   untouched. Application-defined sections, like `MyServiceConfig` below, are not walked
   automatically - call the same expansion yourself with the `string` extension `ReplaceEnv()`
   (`SquidStd.Core.Extensions.Env`) wherever you read the value.
4. **Read values.** Resolve the config type from the container wherever you need it - the
   value is available as soon as the registration call returns.

```csharp
public sealed class MyServiceConfig
{
    public string Endpoint { get; set; } = string.Empty; // e.g. "https://$API_HOST"
    public int Retries { get; set; } = 3;

    // MyServiceConfig lives outside the SquidStd namespace, so it is not walked by the
    // automatic substitution pass. Expand tokens explicitly where you read the value:
    // config.Endpoint = config.Endpoint.ReplaceEnv();
}

var bootstrap = SquidStdBootstrap.Create(
    new SquidStdOptions { ConfigName = "squidstd", RootDirectory = AppContext.BaseDirectory });

bootstrap.ConfigureServices(container =>
{
    container.RegisterConfigSection("myService", static () => new MyServiceConfig());

    var config = container.Resolve<MyServiceConfig>(); // already bound, no need to wait for StartAsync

    return container;
});

await bootstrap.StartAsync();
```

The corresponding `squidstd.yaml`:

```yaml
myService:
  endpoint: "https://$API_HOST"
  retries: 5
```

## YAML naming conventions

Section property keys are bound using a `YamlNamingConventionType` (`SquidStd.Core.Types.Yaml`); section
names themselves are never affected - they are matched exactly as registered (the `sectionName` argument
of `RegisterConfigSection`, e.g. `myService` above).

| `YamlNamingConventionType` | Example key for `MaxRetryCount` |
|-----------------------------|----------------------------------|
| `PascalCase` (default)      | `MaxRetryCount`                  |
| `CamelCase`                 | `maxRetryCount`                  |
| `SnakeCase`                 | `max_retry_count`                |
| `KebabCase`                 | `max-retry-count`                |
| `LowerCase`                 | `maxretrycount`                  |

The convention is fixed at load time and applies to every section bound from that `SquidStdConfig`:

- **Through the bootstrap** - `SquidStdOptions.YamlNamingConvention` (default `PascalCase`) is passed to
  the internal `SquidStdConfig.Load(configName, configDirectory, convention)` call the bootstrap makes for
  you. It has no effect when you supply a pre-loaded `SquidStdConfig` via
  `SquidStdBootstrap.Create(SquidStdConfig, SquidStdOptions)` (see the two-phase setup below) - that
  config's own convention always applies, since the file was already loaded before `Create` saw the options.
- **Loading `SquidStdConfig` yourself** - pass the convention as the third argument:
  `SquidStdConfig.Load("squidstd", "~/.squidstd", YamlNamingConventionType.SnakeCase)`.
- **Registering against a container directly** - `RegisterConfigServices(configName, configDirectory,
  convention)` and `RegisterConfigManagerService(configName, configDirectory, convention)` both accept the
  same trailing `convention` parameter (default `PascalCase`).

Every path through `SquidStdConfig` - `GetSection`, `BindSection`, `HasSection`, `Compose`, `Save` - uses
the convention recorded at `Load` time, so binding a section and later re-serializing it with `Compose()`
or `Save()` round-trip through the same casing.

A `squidstd.yaml` written in `SnakeCase`:

```yaml
network:
  bind_address: 0.0.0.0
  max_connections: 128
```

binds against a plain, PascalCase-declared C# type once the option is set:

```csharp
public sealed class NetworkConfig
{
    public string BindAddress { get; set; } = string.Empty;
    public int MaxConnections { get; set; }
}

var bootstrap = SquidStdBootstrap.Create(
    new SquidStdOptions
    {
        ConfigName = "squidstd",
        RootDirectory = AppContext.BaseDirectory,
        YamlNamingConvention = YamlNamingConventionType.SnakeCase
    });

bootstrap.ConfigureServices(container =>
{
    container.RegisterConfigSection("network", static () => new NetworkConfig());
    return container;
});
```

The `network` section name is untouched by the convention - only its property keys
(`BindAddress`/`bind_address`, `MaxConnections`/`max_connections`) are. `YamlUtils.Deserialize`,
`Serialize`, `SerializeSections`, and the standalone `YamlDataSerializer` (the `IDataSerializer` /
`IDataDeserializer` implementation behind `RegisterYamlDataSerializer()`) accept the same enum as a
trailing parameter and default to `PascalCase` too.

## Four ways to configure a service

Sectioned registrations (`RegisterEventLoop`, `RegisterJobSystemService`,
`RegisterTimerWheelService`, `RegisterMetricsCollectionService`, `RegisterSecretServices`, and
the other `RegisterXxx`/`AddXxx` helpers) accept an optional explicit config instance, so there
are four ways to supply a value, from least to most code:

1. **Standard section from file** - call the registration with no config; it binds the
   named YAML section (or the `createDefault` factory when the section is absent):

   ```csharp
   container.RegisterEventLoop(); // binds the "eventLoop" section
   ```

2. **`OnConfigLoaded<T>` hook** - keep the standard binding, but adjust the bound instance
   once at startup:

   ```csharp
   bootstrap.OnConfigLoaded<EventLoopConfig>(c => c.IdleSleepMs = 5);
   ```

3. **Derived from another section** - bind a parent section yourself and pass one of its
   nested values through:

   ```csharp
   var appConfig = config.GetSection<MyServerConfig>("myapp");
   container.RegisterEventLoop(appConfig.EventLoop);
   ```

4. **Explicit instance** - bypass the file entirely:

   ```csharp
   container.RegisterEventLoop(new EventLoopConfig { IdleSleepMs = 0 });
   ```

Options 3 and 4 register the instance directly (`IfAlreadyRegistered.Replace`) and skip
`RegisterConfigSection`, so the YAML file is never read for that section.

## Two-phase setup (Moongate-style)

Apps that need configuration before any service is registered - to size worker pools, pick a
storage backend, and so on - load the `SquidStdConfig` themselves and pass it to `Create`:

```csharp
// Phase 1 - config loaded immediately, no container involved
var config = SquidStdConfig.Load("moongate", "~/.moongate");

// Phase 2 - registrations with real config objects
var bootstrap = SquidStdBootstrap.Create(
        config,
        new SquidStdOptions { ConfigName = "moongate", RootDirectory = "~/.moongate" }
    )
    .ConfigureServices(c =>
    {
        c.RegisterEventLoop(config.GetSection<EventLoopConfig>("eventLoop"));                     // from file, direct
        c.RegisterTimerWheelService(config.GetSection<MoongateServerConfig>("moongate").TimerWheel); // derived
        c.RegisterJobSystemService(new JobsConfig { WorkerThreadCount = 8 });                      // code only
        c.RegisterMetricsCollectionService();                                                      // standard section, binds at registration
        return c;
    });

await bootstrap.RunAsync();
```

## Inspecting and overriding loaded configuration

The config manager binds sections transparently, but nothing stays hidden:

```csharp
var config = bootstrap.Resolve<IConfigManagerService>();
Console.WriteLine(config.Compose());                      // full current configuration as YAML
var logger = config.GetConfig<SquidStdLoggerOptions>();   // one typed section
```

To inspect or tweak a section at startup - before the logger and the services consume it -
register a typed hook on the bootstrap. Hooks run once, right after the configuration is
loaded, and mutate the section in memory only: the YAML file is never rewritten (call `Save()`
explicitly if you want to persist).

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

`OnConfigReady` follows the same rules as the typed hooks: it runs once, after the
configuration is loaded and before the logger and the services, and must be registered
before the bootstrap starts.

## Reloading configuration

Both the typed hooks and `OnConfigReady` also run again on an explicit reload:
`bootstrap.Resolve<IConfigManagerService>().Load()` re-reads the YAML file, re-binds every
tracked section, and re-applies every hook - so overrides made with `OnConfigLoaded` are never
lost across a reload. There is no automatic file watch: call `Load()` yourself when you know
the file changed.

Rebinding replaces the section's registration in the container - a service that already holds
a direct reference to the old instance keeps using it until it resolves the section again.
