using SquidStd.Persistence.Abstractions.Data;

namespace SquidStd.Persistence.Abstractions.Interfaces.Persistence;

/// <summary>Reads and writes per-type entity snapshot files.</summary>
public interface ISnapshotService
{
    ValueTask DeleteBucketAsync(string typeName, ushort typeId, CancellationToken cancellationToken = default);

    ValueTask<PersistedBucket?> LoadBucketAsync(
        string typeName,
        ushort typeId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Loads the bucket, migrating a snapshot still stored under <paramref name="legacyTypeId" />.
    /// </summary>
    /// <remarks>
    /// An overload with a default body rather than a new parameter on the method above, so an
    /// implementation of this interface living outside this repository keeps compiling. An
    /// implementation that stores snapshots its own way simply inherits the no-migration behaviour.
    /// </remarks>
    ValueTask<PersistedBucket?> LoadBucketAsync(
        string typeName,
        ushort typeId,
        ushort? legacyTypeId,
        CancellationToken cancellationToken = default
    )
        => LoadBucketAsync(typeName, typeId, cancellationToken);

    ValueTask SaveBucketAsync(
        EntitySnapshotBucket bucket,
        long lastSequenceId,
        CancellationToken cancellationToken = default
    );
}
