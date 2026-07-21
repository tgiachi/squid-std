using SquidStd.Core.Json;
using SquidStd.Persistence.Abstractions;
using SquidStd.Persistence.Abstractions.Data;
using SquidStd.Persistence.Abstractions.Types;
using SquidStd.Persistence.Abstractions.Types.Persistence;
using SquidStd.Persistence.Data;
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

    [Fact]
    public async Task Replay_TranslatesAnEntryWrittenUnderTheLegacyId()
    {
        // Run 1: the store still has id 1, and the write lands in the journal with no snapshot after it.
        var first = Service(typeId: LegacyId);
        await first.Service.InitializeAsync();
        await first.Service.GetStore<Thing, int>().UpsertAsync(new() { Id = 1, Name = "kept" });
        await first.Service.DisposeAsync();

        // Run 2: the id is now derived, and the store declares where it came from.
        var second = Service(typeId: PersistedTypeId.Derive(StoreName), legacyTypeId: LegacyId);
        await second.Service.InitializeAsync();

        var fetched = await second.Service.GetStore<Thing, int>().GetByIdAsync(1);

        Assert.NotNull(fetched);
        Assert.Equal("kept", fetched!.Name);
    }

    [Fact]
    public async Task Replay_WithAGenuinelyUnknownTypeId_FailsStartup()
    {
        await WriteOrphanJournalEntry();

        var service = Service(typeId: PersistedTypeId.Derive(StoreName), legacyTypeId: LegacyId);

        // Skipping it would discard a write and say so only in a log line nobody reads.
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.Service.InitializeAsync()
        );

        Assert.Contains("40000", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Replay_WithTheOptOut_SkipsAnUnknownTypeId()
    {
        await WriteOrphanJournalEntry();

        var service = Service(
            typeId: PersistedTypeId.Derive(StoreName),
            legacyTypeId: LegacyId,
            skipUnknown: true
        );

        await service.Service.InitializeAsync();
    }

    [Fact]
    public async Task DerivedIds_RoundTripAcrossARestart()
    {
        var derived = PersistedTypeId.Derive(StoreName);

        var first = Service(typeId: derived);
        await first.Service.InitializeAsync();
        await first.Service.GetStore<Thing, int>().UpsertAsync(new() { Id = 5, Name = "round" });
        await first.Service.SaveSnapshotAsync();
        await first.Service.DisposeAsync();

        var second = Service(typeId: derived);
        await second.Service.InitializeAsync();

        Assert.Equal("round", (await second.Service.GetStore<Thing, int>().GetByIdAsync(5))!.Name);
    }

    /// <summary>Appends a journal entry for a type nothing registers, standing in for a removed entity.</summary>
    private async Task WriteOrphanJournalEntry()
    {
        var config = new PersistenceConfig { SaveDirectory = _dir };
        Directory.CreateDirectory(_dir);
        await using var journal = new BinaryJournalService(Path.Combine(_dir, config.JournalFileName));

        await journal.AppendAsync(
            new()
            {
                SequenceId = 1,
                TimestampUnixMilliseconds = 1000,
                TypeId = 40000,
                Operation = JournalEntityOperationType.Upsert,
                Payload = [1, 2, 3]
            }
        );
    }

    private (PersistenceService Service, PersistenceEntityRegistry Registry) Service(
        ushort typeId,
        ushort? legacyTypeId = null,
        bool skipUnknown = false
    )
    {
        var serializer = new JsonDataSerializer();
        var registry = new PersistenceEntityRegistry();

        registry.Register(
            new PersistenceEntityDescriptor<Thing, int>(
                serializer,
                serializer,
                typeId,
                StoreName,
                1,
                thing => thing.Id,
                null,
                null,
                legacyTypeId
            )
        );

        var config = new PersistenceConfig
        {
            SaveDirectory = _dir,
            AutosaveInterval = TimeSpan.FromHours(1),
            SkipUnknownJournalEntries = skipUnknown
        };

        return (
            new PersistenceService(
                registry,
                new BinaryJournalService(Path.Combine(_dir, config.JournalFileName)),
                new SnapshotService(_dir, config.SnapshotFileSuffix),
                config,
                null
            ),
            registry
        );
    }

    private sealed class Thing
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }
    }
}
