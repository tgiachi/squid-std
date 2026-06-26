using SquidStd.Abstractions.Interfaces.Services;

namespace SquidStd.Persistence.Abstractions.Interfaces.Persistence;

/// <summary>
/// Owns persistence lifecycle: loads the snapshot and replays the journal at startup, autosaves
/// periodically, and hands out per-type <see cref="IEntityStore{TEntity,TKey}" /> instances.
/// </summary>
public interface IPersistenceService : ISquidStdService
{
    IEntityStore<TEntity, TKey> GetStore<TEntity, TKey>() where TKey : notnull;
    ValueTask InitializeAsync(CancellationToken cancellationToken = default);
    ValueTask SaveSnapshotAsync(CancellationToken cancellationToken = default);
}
