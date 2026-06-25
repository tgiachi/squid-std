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
        => _snapshot.Metrics;

    public MetricsSnapshot GetSnapshot()
        => _snapshot;

    public MetricsSnapshot GetStatus()
        => _snapshot;
}
