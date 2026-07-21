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
    : IPersistenceEntityDescriptor<TEntity, TKey>, IInternalEntityApplier, IInternalAutoIdDescriptor<TEntity, TKey>
    where TKey : notnull
{
    private readonly IDataDeserializer _deserializer;
    private readonly Func<TEntity, TKey> _keySelector;
    private readonly Action<TEntity, TKey>? _keySetter;
    private readonly IIdGenerator<TKey>? _idGenerator;
    private readonly IDataSerializer _serializer;

    public ushort TypeId { get; }
    public ushort? LegacyTypeId { get; }
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
        Func<TEntity, TKey> keySelector,
        Action<TEntity, TKey>? keySetter = null,
        IIdGenerator<TKey>? idGenerator = null,
        ushort? legacyTypeId = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);
        ArgumentNullException.ThrowIfNull(keySelector);

        if (idGenerator is not null && keySetter is null)
        {
            throw new ArgumentException(
                "An auto-id entity (idGenerator provided) also requires a keySetter to write the generated id.",
                nameof(keySetter)
            );
        }

        _serializer = serializer;
        _deserializer = deserializer;
        TypeId = typeId;
        LegacyTypeId = legacyTypeId;
        TypeName = typeName;
        SchemaVersion = schemaVersion;
        _keySelector = keySelector;
        _keySetter = keySetter;
        _idGenerator = idGenerator;
    }

    public bool IsAutoId => _idGenerator is not null;

    public TKey GetKey(TEntity entity)
        => _keySelector(entity);

    public bool IsDefaultKey(TKey key)
        => EqualityComparer<TKey>.Default.Equals(key, default!);

    public void SetKey(TEntity entity, TKey key)
    {
        if (_keySetter is null)
        {
            throw new InvalidOperationException($"Entity '{TypeName}' has no key setter; cannot assign a generated id.");
        }

        _keySetter(entity, key);
    }

    internal TKey AllocateNextKey(PersistenceStateStore stateStore)
    {
        if (_idGenerator is null)
        {
            throw new InvalidOperationException($"Entity '{TypeName}' is not an auto-id type.");
        }

        var last = stateStore.GetLastKey(TypeId);
        var next = last is null ? _idGenerator.Initial : _idGenerator.Next((TKey)last);
        stateStore.SetLastKey(TypeId, next);

        return next;
    }

    internal void NoteKey(PersistenceStateStore stateStore, TKey key)
    {
        if (_idGenerator is null)
        {
            return;
        }

        var last = stateStore.GetLastKey(TypeId);

        if (last is null || Comparer<TKey>.Default.Compare(key, (TKey)last) > 0)
        {
            stateStore.SetLastKey(TypeId, key);
        }
    }

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
        var key = GetKey(entity);
        stateStore.GetBucket<TEntity, TKey>(TypeId)[key] = entity;
        NoteKey(stateStore, key);
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

        return new()
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

    byte[]? IInternalEntityApplier.SerializeHighWater(PersistenceStateStore stateStore)
    {
        var last = stateStore.GetLastKey(TypeId);

        return last is null ? null : SerializeKey((TKey)last);
    }

    void IInternalEntityApplier.LoadHighWater(PersistenceStateStore stateStore, byte[] payload)
        => stateStore.SetLastKey(TypeId, DeserializeKey(payload));

    TKey IInternalAutoIdDescriptor<TEntity, TKey>.AllocateNextKey(PersistenceStateStore stateStore)
        => AllocateNextKey(stateStore);

    void IInternalAutoIdDescriptor<TEntity, TKey>.NoteKey(PersistenceStateStore stateStore, TKey key)
        => NoteKey(stateStore, key);
}
