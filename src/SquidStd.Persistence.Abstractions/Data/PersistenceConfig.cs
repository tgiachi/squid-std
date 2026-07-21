using SquidStd.Persistence.Abstractions.Types.Persistence;

namespace SquidStd.Persistence.Abstractions.Data;

/// <summary>Configuration for the persistence service: autosave cadence and snapshot/journal file names.</summary>
public sealed class PersistenceConfig
{
    public TimeSpan AutosaveInterval { get; set; } = TimeSpan.FromSeconds(300);
    public string SnapshotFileSuffix { get; set; } = ".snapshot.bin";
    public string JournalFileName { get; set; } = "world.journal.bin";
    public bool EnableFileLock { get; set; } = true;
    public string? SaveDirectory { get; set; }
    public DurabilityMode DurabilityMode { get; set; } = DurabilityMode.Buffered;

    /// <summary>
    /// Whether journal entries whose type id matches no registration are discarded instead of failing
    /// startup. Default false: a skipped entry is a silently lost write, and the usual cause is a
    /// renamed or removed entity, which the operator should decide about deliberately.
    /// </summary>
    public bool SkipUnknownJournalEntries { get; set; } = false;
}
