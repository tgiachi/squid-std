using SquidStd.Core.Interfaces.Events;

namespace SquidStd.Persistence.Abstractions.Data.Events;

/// <summary>Raised after a snapshot save completes and the journal is trimmed.</summary>
public sealed record SnapshotSaveCompletedEvent(long SequenceId, int BucketCount) : IEvent;
