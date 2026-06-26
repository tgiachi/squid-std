using SquidStd.Persistence.Abstractions.Data;

namespace SquidStd.Persistence.Abstractions.Interfaces.Persistence;

/// <summary>Appends and replays journal entries from durable storage.</summary>
public interface IJournalService
{
    ValueTask AppendAsync(JournalEntry entry, CancellationToken cancellationToken = default);
    ValueTask AppendBatchAsync(IReadOnlyList<JournalEntry> entries, CancellationToken cancellationToken = default);
    ValueTask<IReadOnlyCollection<JournalEntry>> ReadAllAsync(CancellationToken cancellationToken = default);
    ValueTask ResetAsync(CancellationToken cancellationToken = default);
    ValueTask TrimThroughSequenceAsync(long inclusiveSequenceId, CancellationToken cancellationToken = default);
}
