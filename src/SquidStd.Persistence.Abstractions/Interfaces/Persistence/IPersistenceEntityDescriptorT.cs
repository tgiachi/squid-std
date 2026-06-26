namespace SquidStd.Persistence.Abstractions.Interfaces.Persistence;

/// <summary>Typed serialization contract for a persisted entity descriptor.</summary>
public interface IPersistenceEntityDescriptor<TEntity, TKey> : IPersistenceEntityDescriptor
    where TKey : notnull
{
    TKey GetKey(TEntity entity);
    TEntity Clone(TEntity entity);
    byte[] SerializeEntity(TEntity entity);
    TEntity DeserializeEntity(byte[] payload);
    byte[] SerializeBucket(IReadOnlyCollection<TEntity> entities);
    IReadOnlyList<TEntity> DeserializeBucket(byte[] payload);
    byte[] SerializeKey(TKey key);
    TKey DeserializeKey(byte[] payload);
}
