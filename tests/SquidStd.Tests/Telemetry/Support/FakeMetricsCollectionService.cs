using SquidStd.Core.Data.Metrics;
using SquidStd.Core.Interfaces.Metrics;

namespace SquidStd.Tests.Telemetry.Support;

/// <summary>Returns a fixed metrics snapshot for telemetry bridge tests.</summary>
public sealed class FakeMetricsCollectionService : IMetricsCollectionService
{
    private readonly MetricsSnapshot _snapshot;

    public FakeMetricsCollectionService(IReadOnlyDictionary<string, MetricSample> metrics)
    {
        _snapshot = new MetricsSnapshot(DateTimeOffset.UnixEpoch, metrics);
    }

    public IReadOnlyDictionary<string, MetricSample> GetAllMetrics()
    {
        return _snapshot.Metrics;
    }

    public MetricsSnapshot GetSnapshot()
    {
        return _snapshot;
    }

    public MetricsSnapshot GetStatus()
    {
        return _snapshot;
    }
}
