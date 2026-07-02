<h1 align="center">SquidStd.AspNetCore</h1>

ASP.NET Core integration for SquidStd. A single `builder.UseSquidStd(...)` call wires the SquidStd
DryIoc container into the web host and registers a hosted service that starts and stops every
`ISquidStdService` alongside the application lifecycle. The bootstrap registers only the configuration
core; opt into the core services with `RegisterCoreServices()` in the container callback.

## Install

```bash
dotnet add package SquidStd.AspNetCore
```

## Usage

```csharp
using SquidStd.AspNetCore.Extensions;
using SquidStd.Services.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.UseSquidStd(
    options =>
    {
        // configure SquidStd options here
    },
    container => container.RegisterCoreServices());

var app = builder.Build();
app.Run();
```

### Health checks

Bridge your SquidStd health checks into the standard `/health` endpoint:

```csharp
using SquidStd.AspNetCore.Extensions;
using SquidStd.Services.Core.Extensions;

builder.UseSquidStd(
    options => { },
    container => container.RegisterCoreServices().RegisterHealthChecksService());
builder.AddSquidStdHealthChecks(); // call after UseSquidStd

var app = builder.Build();
app.MapHealthChecks("/health"); // standard ASP.NET Core endpoint
```

Each registered `IHealthCheck` appears as its own entry in the report. Check names must be unique.

## Key types

| Type                                  | Purpose                                                                |
|---------------------------------------|------------------------------------------------------------------------|
| `SquidStdAspNetCoreBuilderExtensions` | `UseSquidStd(...)` builder extension.                                  |
| `SquidStdHostedService`               | Hosted service bridging SquidStd service lifecycle to the host.        |
| `SquidStdHealthChecksExtensions`      | `AddSquidStdHealthChecks(...)` - bridge to ASP.NET Core health checks. |

## Unified logging (opt-in)

By default the SquidStd Serilog logger (configured from the `logger` section of `squidstd.yaml`) and the
ASP.NET Core framework logger run as two separate pipelines, producing two console formats. Call
`AddSquidStdSerilog()` after `UseSquidStd()` to route framework logging through SquidStd's Serilog logger,
so everything shares one configuration and one format:

```csharp
builder.UseSquidStd(options => options.ConfigName = "squidstd", c => c.RegisterCoreServices());
builder.AddSquidStdSerilog();
```

The logger is driven entirely by `squidstd.yaml` (keys are PascalCase and case-sensitive):

```yaml
logger:
  MinimumLevel: Information   # None disables all logging, framework included
  EnableConsole: true
  EnableFile: false
  LogDirectory: logs
  FileName: squidstd-.log
  RollingInterval: Day
```

## Related

- Tutorial: [Build an ASP.NET Core app](https://tgiachi.github.io/squid-std/tutorials/aspnetcore-app.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
