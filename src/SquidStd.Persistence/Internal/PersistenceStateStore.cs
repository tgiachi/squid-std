namespace SquidStd.Persistence.Internal;

/// <summary>
/// In-memory mutable state shared by entity stores. Dictionary access is guarded by <see cref="SyncRoot" />;
/// writers serialize end-to-end (apply + journal append) on <see cref="WriteLock" /> so journal order
/// matches sequence order.
/// </summary>
internal sealed class PersistenceStateStore
{
    private readonly Dictionary<ushort, object> _entityBuckets = [];
    private readonly Dictionary<ushort, object> _lastKeys = [];

    public object SyncRoot { get; } = new();

    public SemaphoreSlim WriteLock { get; } = new(1, 1);

    public long LastSequenceId { get; set; }

    public Dictionary<TKey, TEntity> GetBucket<TEntity, TKey>(ushort typeId)
        where TKey : notnull
    {
        if (_entityBuckets.TryGetValue(typeId, out var existing))
        {
            return (Dictionary<TKey, TEntity>)existing;
        }

        var created = new Dictionary<TKey, TEntity>();
        _entityBuckets[typeId] = created;

        return created;
    }

    public object? GetLastKey(ushort typeId)
        => _lastKeys.GetValueOrDefault(typeId);

    public void SetLastKey(ushort typeId, object key)
        => _lastKeys[typeId] = key;

    public void ClearBuckets()
    {
        _entityBuckets.Clear();
        _lastKeys.Clear();
    }
}
