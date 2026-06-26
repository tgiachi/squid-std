using SquidStd.Core.Interfaces.Events;

namespace SquidStd.Persistence.Abstractions.Data.Events;

/// <summary>Raised before a snapshot save begins.</summary>
public sealed record SnapshotSaveStartedEvent(long SequenceId) : IEvent;
