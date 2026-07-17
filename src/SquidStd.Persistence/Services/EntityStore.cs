using SquidStd.Persistence.Abstractions.Data;
using SquidStd.Persistence.Abstractions.Interfaces.Persistence;
using SquidStd.Persistence.Abstractions.Types;
using SquidStd.Persistence.Internal;

namespace SquidStd.Persistence.Services;

/// <summary>
/// In-memory <see cref="IEntityStore{TEntity,TKey}" /> backed by the shared state store. Reads return
/// detached clones; writes serialize end-to-end on the state store's write lock (apply + journal append),
/// so journal order matches sequence order.
/// </summary>
public sealed class EntityStore<TEntity, TKey> : IEntityStore<TEntity, TKey>
    where TKey : notnull
{
    private readonly IPersistenceEntityDescriptor<TEntity, TKey> _descriptor;
    private readonly IJournalService _journalService;
    private readonly PersistenceStateStore _stateStore;

    internal EntityStore(
        PersistenceStateStore stateStore,
        IJournalService journalService,
        IPersistenceEntityDescriptor<TEntity, TKey> descriptor
    )
    {
        _stateStore = stateStore;
        _journalService = journalService;
        _descriptor = descriptor;
    }

    public int Count()
    {
        lock (_stateStore.SyncRoot)
        {
            return Bucket().Count;
        }
    }

    public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
        => ValueTask.FromResult(Count());

    public IReadOnlyCollection<TEntity> GetAll()
    {
        lock (_stateStore.SyncRoot)
        {
            IReadOnlyCollection<TEntity> clones = [.. Bucket().Values.Select(_descriptor.Clone)];

            return clones;
        }
    }

    public ValueTask<IReadOnlyCollection<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => ValueTask.FromResult(GetAll());

    public TEntity? GetById(TKey id)
    {
        lock (_stateStore.SyncRoot)
        {
            return Bucket().TryGetValue(id, out var entity) ? _descriptor.Clone(entity) : default;
        }
    }

    public ValueTask<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
        => ValueTask.FromResult(GetById(id));

    public IQueryable<TEntity> Query()
    {
        lock (_stateStore.SyncRoot)
        {
            return Bucket().Values.Select(_descriptor.Clone).ToArray().AsQueryable();
        }
    }

    public PagedResult<TEntity> QueryPaged<TOrder>(
        Func<TEntity, bool>? filter,
        Func<TEntity, TOrder> orderBy,
        int skip,
        int take,
        bool descending = false
    )
    {
        lock (_stateStore.SyncRoot)
        {
            // Filtering and ordering run against the live entities, not clones: that is the whole saving.
            // Only what survives skip/take is cloned, so the cost is O(bucket) comparisons plus O(page)
            // clones instead of O(bucket) clones.
            IEnumerable<TEntity> matching = Bucket().Values;

            if (filter is not null)
            {
                matching = matching.Where(filter);
            }

            var matched = matching as IList<TEntity> ?? [.. matching];

            // The entity key breaks ties. The caller's key is rarely unique, and leaving equal keys to
            // Dictionary order is exactly the instability orderBy exists to remove.
            var ordered = descending
                              ? matched.OrderByDescending(orderBy).ThenByDescending(_descriptor.GetKey)
                              : matched.OrderBy(orderBy).ThenBy(_descriptor.GetKey);

            IReadOnlyList<TEntity> page = [.. ordered.Skip(skip).Take(take).Select(_descriptor.Clone)];

            return new(page, matched.Count, skip, take);
        }
    }

    public async ValueTask UpsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _stateStore.WriteLock.WaitAsync(cancellationToken);

        try
        {
            JournalEntry entry;
            TEntity clone;
            TKey key;
            long next;

            lock (_stateStore.SyncRoot)
            {
                clone = _descriptor.Clone(entity);
                key = _descriptor.GetKey(clone);

                if (_descriptor is IInternalAutoIdDescriptor<TEntity, TKey> autoId && autoId.IsAutoId)
                {
                    if (autoId.IsDefaultKey(key))
                    {
                        key = autoId.AllocateNextKey(_stateStore);
                        autoId.SetKey(clone, key);
                        // Assign back onto the caller's instance so it observes the generated id.
                        autoId.SetKey(entity, key);
                    }
                    else
                    {
                        autoId.NoteKey(_stateStore, key);
                    }
                }

                next = _stateStore.LastSequenceId + 1; // computed, not yet committed
                entry = new()
                {
                    SequenceId = next,
                    TimestampUnixMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    TypeId = _descriptor.TypeId,
                    Operation = JournalEntityOperationType.Upsert,
                    Payload = _descriptor.SerializeEntity(clone)
                };
            }

            // Write-ahead: the journal must be durable before the in-memory state reflects the change.
            await _journalService.AppendAsync(entry, cancellationToken);

            lock (_stateStore.SyncRoot)
            {
                Bucket()[key] = clone;
                _stateStore.LastSequenceId = next;
            }
        }
        finally
        {
            _stateStore.WriteLock.Release();
        }
    }

    public async ValueTask<bool> RemoveAsync(TKey id, CancellationToken cancellationToken = default)
    {
        await _stateStore.WriteLock.WaitAsync(cancellationToken);

        try
        {
            JournalEntry entry;
            long next;

            lock (_stateStore.SyncRoot)
            {
                if (!Bucket().ContainsKey(id))
                {
                    return false;
                }

                next = _stateStore.LastSequenceId + 1;
                entry = new()
                {
                    SequenceId = next,
                    TimestampUnixMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    TypeId = _descriptor.TypeId,
                    Operation = JournalEntityOperationType.Remove,
                    Payload = _descriptor.SerializeKey(id)
                };
            }

            // Write-ahead: append before applying so a failed append leaves the entity in place.
            await _journalService.AppendAsync(entry, cancellationToken);

            lock (_stateStore.SyncRoot)
            {
                Bucket().Remove(id);
                _stateStore.LastSequenceId = next;
            }

            return true;
        }
        finally
        {
            _stateStore.WriteLock.Release();
        }
    }

    private Dictionary<TKey, TEntity> Bucket()
        => _stateStore.GetBucket<TEntity, TKey>(_descriptor.TypeId);
}
