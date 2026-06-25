using OpenTelemetry;
using OpenTelemetry.Metrics;
using SquidStd.Core.Data.Metrics;
using SquidStd.Telemetry.OpenTelemetry.Services;
using SquidStd.Tests.Telemetry.Support;
using OtelMetricType = OpenTelemetry.Metrics.MetricType;
using SquidMetricType = SquidStd.Core.Types.Metrics.MetricType;

namespace SquidStd.Tests.Telemetry;

public class MetricsSnapshotBridgeTests
{
    [Fact]
    public void Bridge_ExportsSnapshotSamplesAsInstruments()
    {
        var metrics = new FakeMetricsCollectionService(
            new Dictionary<string, MetricSample>
            {
                ["svc.calls"] = new("svc.calls", 5, Type: SquidMetricType.Counter),
                ["svc.queue"] = new("svc.queue", 3, Type: SquidMetricType.Gauge)
            }
        );

        var exported = new List<Metric>();
        using var bridge = new MetricsSnapshotBridge(metrics);

        using (var provider = Sdk.CreateMeterProviderBuilder()
                                 .AddMeter(MetricsSnapshotBridge.MeterName)
                                 .AddInMemoryExporter(exported)
                                 .Build())
        {
            provider.ForceFlush();
        }

        Assert.Equal(5, ReadValue(exported, "svc.calls"));
        Assert.Equal(3, ReadValue(exported, "svc.queue"));
    }

    private static double ReadValue(List<Metric> metrics, string name)
    {
        var metric = metrics.Last(m => m.Name == name);

        foreach (ref readonly var point in metric.GetMetricPoints())
        {
            return metric.MetricType == OtelMetricType.DoubleGauge
                       ? point.GetGaugeLastValueDouble()
                       : point.GetSumDouble();
        }

        return double.NaN;
    }
}
