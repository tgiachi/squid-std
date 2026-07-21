using SquidStd.Persistence.Abstractions;
using SquidStd.Persistence.Abstractions.Data;
using SquidStd.Persistence.Abstractions.Types.Persistence;
using SquidStd.Persistence.Services;

namespace SquidStd.Tests.Persistence;

public sealed class LegacyTypeIdMigrationTests : IDisposable
{
    private const string StoreName = "accounts";
    private const ushort LegacyId = 1;

    private readonly string _dir =
        Path.Combine(Path.GetTempPath(), "squidstd-legacy-id-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task LoadBucket_RenamesASnapshotWrittenUnderTheLegacyId()
    {
        var service = new SnapshotService(_dir, ".snapshot.bin", DurabilityMode.Durable);
        var derived = PersistedTypeId.Derive(StoreName);

        // A snapshot saved back when this store's id was still 1.
        await service.SaveBucketAsync(
            new EntitySnapshotBucket
            {
                TypeId = LegacyId,
                TypeName = StoreName,
                SchemaVersion = 1,
                Payload = [7, 7, 7]
            },
            lastSequenceId: 42
        );

        var loaded = await service.LoadBucketAsync(StoreName, derived, LegacyId);

        Assert.NotNull(loaded);
        Assert.Equal(42, loaded!.LastSequenceId);
        Assert.Equal(new byte[] { 7, 7, 7 }, loaded.Bucket.Payload);

        // Renamed, not copied: the migration happens once and leaves nothing behind to load twice.
        Assert.True(File.Exists(Path.Combine(_dir, $"{StoreName}_{derived}.snapshot.bin")));
        Assert.False(File.Exists(Path.Combine(_dir, $"{StoreName}_{LegacyId}.snapshot.bin")));
    }

    [Fact]
    public async Task LoadBucket_WithNothingOnDisk_IsStillNull()
    {
        var service = new SnapshotService(_dir, ".snapshot.bin", DurabilityMode.Durable);

        // The ordinary first-boot path must be unaffected by the migration branch.
        Assert.Null(await service.LoadBucketAsync(StoreName, PersistedTypeId.Derive(StoreName), LegacyId));
    }

    [Fact]
    public async Task LoadBucket_WhenTheDerivedFileAlreadyExists_DoesNotTouchTheLegacyOne()
    {
        var service = new SnapshotService(_dir, ".snapshot.bin", DurabilityMode.Durable);
        var derived = PersistedTypeId.Derive(StoreName);

        await service.SaveBucketAsync(
            new EntitySnapshotBucket { TypeId = derived, TypeName = StoreName, SchemaVersion = 1, Payload = [1] },
            lastSequenceId: 10
        );
        await File.WriteAllBytesAsync(Path.Combine(_dir, $"{StoreName}_{LegacyId}.snapshot.bin"), [0xBA, 0xD0]);

        var loaded = await service.LoadBucketAsync(StoreName, derived, LegacyId);

        // A stale legacy file must never overwrite current data.
        Assert.Equal(new byte[] { 1 }, loaded!.Bucket.Payload);
        Assert.True(File.Exists(Path.Combine(_dir, $"{StoreName}_{LegacyId}.snapshot.bin")));
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }
    }
}
