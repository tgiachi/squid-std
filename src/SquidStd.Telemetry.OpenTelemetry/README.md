<h1 align="center">SquidStd.Telemetry.OpenTelemetry</h1>

OpenTelemetry tracing and metrics export for SquidStd. Adds distributed tracing (ASP.NET Core +
HttpClient + the `SquidStd.*` ActivitySource), runtime metrics, and a bridge that exports the existing
SquidStd metrics snapshot - all via OTLP (+ optional console). Logs stay on Serilog.

## Install

```bash
dotnet add package SquidStd.Telemetry.OpenTelemetry
```

## Usage

Worker / DryIoc host:

```csharp
using SquidStd.Telemetry.Abstractions.Data.Config;
using SquidStd.Telemetry.OpenTelemetry.Extensions;

container.AddSquidStdTelemetry(new TelemetryOptions
{
    ServiceName = "orders-worker",
    OtlpEndpoint = "http://localhost:4317",
});
```

ASP.NET Core host:

```csharp
builder.Services.AddSquidStdTelemetry(new TelemetryOptions { ServiceName = "orders-api" });
```

- Tracing: ASP.NET Core (incoming, web host only) + HttpClient (outgoing) + every `SquidStd.*`
  ActivitySource; ParentBased ratio sampler.
- Metrics: runtime instrumentation + the SquidStd metrics snapshot bridged to OTel instruments.
- Exporters: OTLP (gRPC/HTTP) plus an optional console exporter for development.

## Key types

| Type                                       | Purpose                                                                     |
|--------------------------------------------|-----------------------------------------------------------------------------|
| `OpenTelemetryContainerExtensions`         | `AddSquidStdTelemetry(...)` for the DryIoc/worker host.                     |
| `OpenTelemetryServiceCollectionExtensions` | `AddSquidStdTelemetry(...)` for the ASP.NET Core host.                      |
| `TelemetryService`                         | Configures tracing/metrics providers and exporters from `TelemetryOptions`. |
| `MetricsSnapshotBridge`                    | Exports the existing SquidStd metrics snapshot to OTel instruments.         |

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
