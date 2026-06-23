using SquidStd.Core.Json;
using SquidStd.Workers.Abstractions;
using SquidStd.Workers.Abstractions.Data;
using SquidStd.Workers.Abstractions.Types;

namespace SquidStd.Tests.Workers;

public class WorkerContractsTests
{
    private static readonly JsonDataSerializer Serializer = new();

    [Fact]
    public void JobRequest_RoundTrips_PreservingNameAndParameters()
    {
        var original = new JobRequest(
            "resize-image",
            new Dictionary<string, string> { ["width"] = "800", ["height"] = "600" });

        var bytes = Serializer.Serialize(original);
        var restored = Serializer.Deserialize<JobRequest>(bytes);

        Assert.Equal("resize-image", restored.JobName);
        Assert.Equal("800", restored.Parameters["width"]);
        Assert.Equal("600", restored.Parameters["height"]);
    }

    [Fact]
    public void WorkerHeartbeat_RoundTrips_WithCurrentJob()
    {
        var original = new WorkerHeartbeat(
            "worker-1",
            new DateTime(2026, 6, 23, 10, 0, 0, DateTimeKind.Utc),
            WorkerStatusType.Busy,
            "resize-image");

        var restored = Serializer.Deserialize<WorkerHeartbeat>(Serializer.Serialize(original));

        Assert.Equal(original, restored);
    }

    [Fact]
    public void WorkerHeartbeat_RoundTrips_WhenIdleWithNullJob()
    {
        var original = new WorkerHeartbeat(
            "worker-2",
            new DateTime(2026, 6, 23, 10, 5, 0, DateTimeKind.Utc),
            WorkerStatusType.Idle,
            null);

        var restored = Serializer.Deserialize<WorkerHeartbeat>(Serializer.Serialize(original));

        Assert.Equal(original, restored);
        Assert.Null(restored.CurrentJob);
    }

    [Theory]
    [InlineData(WorkerStatusType.Idle)]
    [InlineData(WorkerStatusType.Busy)]
    [InlineData(WorkerStatusType.Offline)]
    public void WorkerHeartbeat_RoundTrips_EveryStatus(WorkerStatusType status)
    {
        var original = new WorkerHeartbeat(
            "worker-3",
            new DateTime(2026, 6, 23, 11, 0, 0, DateTimeKind.Utc),
            status,
            null);

        var restored = Serializer.Deserialize<WorkerHeartbeat>(Serializer.Serialize(original));

        Assert.Equal(status, restored.Status);
    }

    [Fact]
    public void WorkerInfo_RoundTrips_PreservingAllFields()
    {
        var original = new WorkerInfo(
            "worker-4",
            WorkerStatusType.Offline,
            new DateTime(2026, 6, 23, 9, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 6, 23, 9, 30, 0, DateTimeKind.Utc),
            null);

        var restored = Serializer.Deserialize<WorkerInfo>(Serializer.Serialize(original));

        Assert.Equal(original, restored);
    }

    [Fact]
    public void WorkerChannels_NamesAreNonEmptyAndDistinct()
    {
        Assert.False(string.IsNullOrWhiteSpace(WorkerChannels.JobQueue));
        Assert.False(string.IsNullOrWhiteSpace(WorkerChannels.HeartbeatTopic));
        Assert.NotEqual(WorkerChannels.JobQueue, WorkerChannels.HeartbeatTopic);
    }
}
