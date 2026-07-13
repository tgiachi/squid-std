namespace SquidStd.Persistence.Abstractions.Interfaces.Persistence;

/// <summary>
/// In-memory CRUD over a registered persisted entity type. Reads complete synchronously from memory
/// and return detached clones; writes append to the journal then apply to memory. Reads are available
/// in both synchronous and asynchronous forms; the async overloads delegate to the synchronous ones.
/// </summary>
/// <remarks>
/// WARNING: do not register or resolve <see cref="IEntityStore{TEntity,TKey}" /> directly in the DI
/// container - it is intentionally never registered there, and its implementation cannot be constructed
/// by the container. Register the entity with <c>RegisterPersistedEntity</c> and obtain the store from
/// <see cref="IPersistenceService.GetStore{TEntity,TKey}" /> instead.
/// </remarks>
public interface IEntityStore<TEntity, in TKey>
{
    /// <summary>Completes synchronously from memory. Returns the number of entities of this type currently held.</summary>
    int Count();

    ValueTask<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>Completes synchronously from memory. Returns a detached clone of every entity of this type.</summary>
    IReadOnlyCollection<TEntity> GetAll();

    ValueTask<IReadOnlyCollection<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Completes synchronously from memory. Returns a detached clone of the entity with <paramref name="id"/>, or default when missing.</summary>
    TEntity? GetById(TKey id);

    ValueTask<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    IQueryable<TEntity> Query();
    ValueTask<bool> RemoveAsync(TKey id, CancellationToken cancellationToken = default);
    ValueTask UpsertAsync(TEntity entity, CancellationToken cancellationToken = default);
}
