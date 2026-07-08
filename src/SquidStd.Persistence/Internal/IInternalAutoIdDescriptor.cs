namespace SquidStd.Persistence.Internal;

/// <summary>
/// Internal generic view of a descriptor's auto-id behavior, used by <c>EntityStore</c> to allocate
/// and record keys. Kept separate from the public descriptor interface because its members touch the
/// internal <see cref="PersistenceStateStore" />.
/// </summary>
internal interface IInternalAutoIdDescriptor<TEntity, TKey>
    where TKey : notnull
{
    bool IsAutoId { get; }
    bool IsDefaultKey(TKey key);
    void SetKey(TEntity entity, TKey key);
    TKey AllocateNextKey(PersistenceStateStore stateStore);
    void NoteKey(PersistenceStateStore stateStore, TKey key);
}
