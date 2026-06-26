namespace SquidStd.Persistence.Abstractions.Data;

/// <summary>A loaded per-type snapshot: the entity bucket plus the sequence id it was written at.</summary>
public sealed record PersistedBucket(EntitySnapshotBucket Bucket, long LastSequenceId);
