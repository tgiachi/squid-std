namespace SquidStd.Persistence.Abstractions.Data;

/// <summary>On-disk container for a single entity type's snapshot file.</summary>
public sealed class SnapshotFileEnvelope
{
    public int Version { get; set; } = 1;
    public long LastSequenceId { get; set; }
    public uint Checksum { get; set; }
    public EntitySnapshotBucket Bucket { get; set; } = new();
}
