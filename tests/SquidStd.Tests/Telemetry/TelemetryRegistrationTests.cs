using DryIoc;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using SquidStd.Abstractions.Interfaces.Services;
using SquidStd.Core.Data.Metrics;
using SquidStd.Core.Interfaces.Metrics;
using SquidStd.Telemetry.Abstractions.Data.Config;
using SquidStd.Telemetry.OpenTelemetry.Extensions;
using SquidStd.Telemetry.OpenTelemetry.Services;
using SquidStd.Tests.Telemetry.Support;

namespace SquidStd.Tests.Telemetry;

public class TelemetryRegistrationTests
{
    [Fact]
    public async Task Container_RegistersTelemetryServiceAndStartsCleanly()
    {
        using var container = new Container();
        container.RegisterInstance<IMetricsCollectionService>(
            new FakeMetricsCollectionService(new Dictionary<string, MetricSample>())
        );

        container.AddSquidStdTelemetry(new TelemetryOptions { EnableConsoleExporter = false });

        var service = container.Resolve<TelemetryService>();
        Assert.IsAssignableFrom<ISquidStdService>(service);

        await service.StartAsync();
        await service.StopAsync();
    }

    [Fact]
    public void ServiceCollection_RegistersTracerAndMeterProviders()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IMetricsCollectionService>(
            new FakeMetricsCollectionService(new Dictionary<string, MetricSample>())
        );

        services.AddSquidStdTelemetry(new TelemetryOptions());

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService(typeof(TracerProvider)));
        Assert.NotNull(provider.GetService(typeof(MeterProvider)));
    }
}
