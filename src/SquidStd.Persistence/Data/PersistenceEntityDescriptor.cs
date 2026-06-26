using SquidStd.Core.Interfaces.Serialization;
using SquidStd.Persistence.Abstractions.Data;
using SquidStd.Persistence.Abstractions.Interfaces.Persistence;
using SquidStd.Persistence.Internal;

namespace SquidStd.Persistence.Data;

/// <summary>
/// Default descriptor for a persisted entity type. Serializes via the injected
/// <see cref="IDataSerializer" />/<see cref="IDataDeserializer" />; <see cref="Clone" /> is a
/// serialize-then-deserialize deep copy for snapshot isolation.
/// </summary>
public sealed class PersistenceEntityDescriptor<TEntity, TKey>
    : IPersistenceEntityDescriptor<TEntity, TKey>, IInternalEntityApplier
    where TKey : notnull
{
    private readonly IDataDeserializer _deserializer;
    private readonly Func<TEntity, TKey> _keySelector;
    private readonly IDataSerializer _serializer;

    public ushort TypeId { get; }
    public string TypeName { get; }
    public int SchemaVersion { get; }
    public Type EntityType => typeof(TEntity);
    public Type KeyType => typeof(TKey);

    public PersistenceEntityDescriptor(
        IDataSerializer serializer,
        IDataDeserializer deserializer,
        ushort typeId,
        string typeName,
        int schemaVersion,
        Func<TEntity, TKey> keySelector
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);
        ArgumentNullException.ThrowIfNull(keySelector);

        _serializer = serializer;
        _deserializer = deserializer;
        TypeId = typeId;
        TypeName = typeName;
        SchemaVersion = schemaVersion;
        _keySelector = keySelector;
    }

    public TKey GetKey(TEntity entity)
        => _keySelector(entity);

    public TEntity Clone(TEntity entity)
        => DeserializeEntity(SerializeEntity(entity));

    public byte[] SerializeEntity(TEntity entity)
        => _serializer.Serialize(entity).ToArray();

    public TEntity DeserializeEntity(byte[] payload)
        => _deserializer.Deserialize<TEntity>(payload);

    public byte[] SerializeBucket(IReadOnlyCollection<TEntity> entities)
        => _serializer.Serialize(entities).ToArray();

    public IReadOnlyList<TEntity> DeserializeBucket(byte[] payload)
        => _deserializer.Deserialize<List<TEntity>>(payload);

    public byte[] SerializeKey(TKey key)
        => _serializer.Serialize(key).ToArray();

    public TKey DeserializeKey(byte[] payload)
        => _deserializer.Deserialize<TKey>(payload);

    void IInternalEntityApplier.ApplyUpsert(PersistenceStateStore stateStore, byte[] payload)
    {
        var entity = DeserializeEntity(payload);
        stateStore.GetBucket<TEntity, TKey>(TypeId)[GetKey(entity)] = entity;
    }

    void IInternalEntityApplier.ApplyRemove(PersistenceStateStore stateStore, byte[] payload)
        => stateStore.GetBucket<TEntity, TKey>(TypeId).Remove(DeserializeKey(payload));

    EntitySnapshotBucket? IInternalEntityApplier.CaptureBucket(PersistenceStateStore stateStore)
    {
        var entities = stateStore.GetBucket<TEntity, TKey>(TypeId).Values.ToArray();

        if (entities.Length == 0)
        {
            return null;
        }

        return new EntitySnapshotBucket
        {
            TypeId = TypeId,
            TypeName = TypeName,
            SchemaVersion = SchemaVersion,
            Payload = SerializeBucket(entities)
        };
    }

    void IInternalEntityApplier.LoadBucket(PersistenceStateStore stateStore, EntitySnapshotBucket bucket)
    {
        var typed = stateStore.GetBucket<TEntity, TKey>(TypeId);
        typed.Clear();

        foreach (var entity in DeserializeBucket(bucket.Payload))
        {
            typed[GetKey(entity)] = entity;
        }
    }

    int IInternalEntityApplier.Count(PersistenceStateStore stateStore)
        => stateStore.GetBucket<TEntity, TKey>(TypeId).Count;
}
