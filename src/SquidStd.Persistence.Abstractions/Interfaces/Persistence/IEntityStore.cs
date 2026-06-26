namespace SquidStd.Persistence.Abstractions.Interfaces.Persistence;

/// <summary>
/// In-memory CRUD over a registered persisted entity type. Reads complete synchronously from memory
/// and return detached clones; writes append to the journal then apply to memory.
/// </summary>
public interface IEntityStore<TEntity, in TKey>
{
    ValueTask<int> CountAsync(CancellationToken cancellationToken = default);
    ValueTask<IReadOnlyCollection<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    ValueTask<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    IQueryable<TEntity> Query();
    ValueTask<bool> RemoveAsync(TKey id, CancellationToken cancellationToken = default);
    ValueTask UpsertAsync(TEntity entity, CancellationToken cancellationToken = default);
}
