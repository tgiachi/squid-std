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
