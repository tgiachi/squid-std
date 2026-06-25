using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Trace;
using SquidStd.Telemetry.Abstractions.Data.Config;
using SquidStd.Telemetry.OpenTelemetry.Internal;

namespace SquidStd.Tests.Telemetry;

public class TracingTests
{
    [Fact]
    public void Pipeline_ExportsSquidStdSpans()
    {
        var exported = new List<Activity>();
        using var source = new ActivitySource("SquidStd.Test");

        using (var provider = BuildProvider(new TelemetryOptions { ServiceName = "test-svc" }, exported))
        {
            using (var activity = source.StartActivity("do-work"))
            {
                activity?.SetTag("k", "v");
            }

            provider.ForceFlush();
        }

        var span = Assert.Single(exported);
        Assert.Equal("do-work", span.DisplayName);
    }

    [Fact]
    public void Pipeline_WithZeroSampling_RecordsNothing()
    {
        var exported = new List<Activity>();
        using var source = new ActivitySource("SquidStd.Test");

        using (var provider = BuildProvider(new TelemetryOptions { TracingSampleRatio = 0.0 }, exported))
        {
            using (source.StartActivity("dropped"))
            {
            }

            provider.ForceFlush();
        }

        Assert.Empty(exported);
    }

    private static TracerProvider BuildProvider(TelemetryOptions options, List<Activity> sink)
    {
        var builder = Sdk.CreateTracerProviderBuilder();
        TelemetryPipeline.ConfigureTracing(builder, options, includeAspNetCore: false);
        builder.AddInMemoryExporter(sink);

        return builder.Build();
    }
}
