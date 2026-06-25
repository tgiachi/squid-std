using System.Reflection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SquidStd.Telemetry.Abstractions;
using SquidStd.Telemetry.Abstractions.Data.Config;
using SquidStd.Telemetry.Abstractions.Types.Telemetry;

namespace SquidStd.Telemetry.OpenTelemetry.Internal;

/// <summary>
/// Shared OpenTelemetry pipeline configuration, used by both the IContainer and IServiceCollection
/// registration surfaces. Instrumentation/source configuration is split from exporter configuration so
/// tests can reuse the production pipeline and append an in-memory exporter.
/// </summary>
internal static class TelemetryPipeline
{
    public const string MeterName = "SquidStd.Metrics";

    public static ResourceBuilder BuildResource(TelemetryOptions options)
    {
        var version = options.ServiceVersion
                      ?? Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
                      ?? "unknown";

        var builder = ResourceBuilder.CreateDefault().AddService(options.ServiceName, serviceVersion: version);

        if (options.ResourceAttributes is { Count: > 0 })
        {
            builder.AddAttributes(
                options.ResourceAttributes.Select(kv => new KeyValuePair<string, object>(kv.Key, kv.Value))
            );
        }

        return builder;
    }

    public static void ConfigureTracing(TracerProviderBuilder builder, TelemetryOptions options, bool includeAspNetCore)
    {
        builder
            .SetResourceBuilder(BuildResource(options))
            .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(options.TracingSampleRatio)))
            .AddSource($"{SquidStdActivity.SourcePrefix}.*")
            .AddSource(SquidStdActivity.SourcePrefix)
            .AddHttpClientInstrumentation();

        if (includeAspNetCore)
        {
            builder.AddAspNetCoreInstrumentation();
        }
    }

    public static void ConfigureMetrics(MeterProviderBuilder builder, TelemetryOptions options)
    {
        builder
            .SetResourceBuilder(BuildResource(options))
            .AddRuntimeInstrumentation()
            .AddMeter(MeterName);
    }

    public static void AddTraceExporters(TracerProviderBuilder builder, TelemetryOptions options)
    {
        builder.AddOtlpExporter(o =>
        {
            o.Endpoint = new Uri(options.OtlpEndpoint);
            o.Protocol = Map(options.OtlpProtocol);
        });

        if (options.EnableConsoleExporter)
        {
            builder.AddConsoleExporter();
        }
    }

    public static void AddMetricExporters(MeterProviderBuilder builder, TelemetryOptions options)
    {
        builder.AddOtlpExporter(o =>
        {
            o.Endpoint = new Uri(options.OtlpEndpoint);
            o.Protocol = Map(options.OtlpProtocol);
        });

        if (options.EnableConsoleExporter)
        {
            builder.AddConsoleExporter();
        }
    }

    public static OtlpExportProtocol Map(OtlpProtocolType protocol)
        => protocol == OtlpProtocolType.HttpProtobuf ? OtlpExportProtocol.HttpProtobuf : OtlpExportProtocol.Grpc;
}
