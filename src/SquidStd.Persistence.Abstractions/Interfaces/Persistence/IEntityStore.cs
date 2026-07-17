using SquidStd.Persistence.Abstractions.Data;

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

    /// <summary>
    /// Completes synchronously from memory. Filters, orders and pages under the state lock, cloning only
    /// the entities on the returned page — unlike <see cref="GetAll" /> and <see cref="Query" />, whose
    /// cost is a deep clone of the whole store on every call.
    /// </summary>
    /// <param name="filter">
    /// Kept entities, or null for all of them. Runs under the state lock against live, uncloned entities,
    /// so it must be a pure, cheap read: no I/O, no locks, no calls back into the store, and it must not
    /// mutate its argument — a mutation here would change stored state with no journal entry behind it.
    /// </param>
    /// <param name="orderBy">
    /// The sort key. Required, not optional: paging an unordered bucket is meaningless, because
    /// enumeration order is not contractual and shifts as entries come and go, so a page could repeat or
    /// drop an entity with nothing in the result admitting it. Ties break on the entity's own key, which
    /// requires <typeparamref name="TKey" /> to be comparable — every realistic key is, and a key that is
    /// not throws on the first comparison rather than mis-ordering quietly.
    /// </param>
    /// <param name="skip">Entities to skip. Past the end yields an empty page and the true total.</param>
    /// <param name="take">Page size. The last page may hold fewer.</param>
    /// <param name="descending">Reverses <paramref name="orderBy" />. The tiebreak reverses with it.</param>
    PagedResult<TEntity> QueryPaged<TOrder>(
        Func<TEntity, bool>? filter,
        Func<TEntity, TOrder> orderBy,
        int skip,
        int take,
        bool descending = false
    );
    ValueTask<bool> RemoveAsync(TKey id, CancellationToken cancellationToken = default);
    ValueTask UpsertAsync(TEntity entity, CancellationToken cancellationToken = default);
}
