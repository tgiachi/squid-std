using SquidStd.Persistence.Abstractions.Data;

namespace SquidStd.Persistence.Internal;

/// <summary>Internal hooks letting a descriptor apply journal ops and snapshot buckets against the state store.</summary>
internal interface IInternalEntityApplier
{
    bool IsAutoId { get; }
    void ApplyUpsert(PersistenceStateStore stateStore, byte[] payload);
    void ApplyRemove(PersistenceStateStore stateStore, byte[] payload);
    EntitySnapshotBucket? CaptureBucket(PersistenceStateStore stateStore);
    void LoadBucket(PersistenceStateStore stateStore, EntitySnapshotBucket bucket);
    int Count(PersistenceStateStore stateStore);
    byte[]? SerializeHighWater(PersistenceStateStore stateStore);
    void LoadHighWater(PersistenceStateStore stateStore, byte[] payload);
}
