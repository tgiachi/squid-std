using SquidStd.Core.Json;
using SquidStd.Persistence.Abstractions.Interfaces.Persistence;
using SquidStd.Persistence.Data;
using SquidStd.Persistence.Internal;
using SquidStd.Persistence.Services;

namespace SquidStd.Tests.Persistence;

public sealed class EntityStoreAutoIdTests : IAsyncDisposable
{
    private sealed class Doc
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private readonly string _dir = Path.Combine(Path.GetTempPath(), "squidstd-autoid-" + Guid.NewGuid().ToString("N"));
    private readonly BinaryJournalService _journal;
    private readonly PersistenceStateStore _stateStore = new();
    private readonly PersistenceEntityDescriptor<Doc, int> _descriptor;
    private readonly EntityStore<Doc, int> _store;

    public EntityStoreAutoIdTests()
    {
        Directory.CreateDirectory(_dir);
        var serializer = new JsonDataSerializer();
        _descriptor = new PersistenceEntityDescriptor<Doc, int>(
            serializer, serializer, 7, "Doc", 1,
            keySelector: d => d.Id,
            keySetter: (d, id) => d.Id = id,
            idGenerator: IdGenerators.Int32(seed: 1));
        _journal = new(Path.Combine(_dir, "world.journal.bin"));
        _store = new(_stateStore, _journal, _descriptor);
    }

    [Fact]
    public async Task Upsert_WithDefaultKey_AssignsSequentialIds()
    {
        var a = new Doc { Name = "a" };
        var b = new Doc { Name = "b" };

        await _store.UpsertAsync(a);
        await _store.UpsertAsync(b);

        Assert.Equal(1, a.Id);
        Assert.Equal(2, b.Id);
        Assert.NotNull(await _store.GetByIdAsync(1));
        Assert.NotNull(await _store.GetByIdAsync(2));
    }

    [Fact]
    public async Task Upsert_WithExplicitKey_IsRespectedAndAdvancesHighWater()
    {
        await _store.UpsertAsync(new Doc { Id = 50, Name = "manual" });

        var next = new Doc { Name = "auto" };
        await _store.UpsertAsync(next);

        Assert.Equal(51, next.Id);
    }

    [Fact]
    public async Task Replay_FromJournal_ResumesWithoutReusingIds()
    {
        var a = new Doc { Name = "a" };
        var b = new Doc { Name = "b" };
        await _store.UpsertAsync(a); // Id 1
        await _store.UpsertAsync(b); // Id 2

        // Rebuild a fresh store over the SAME journal, replaying entries as PersistenceService would.
        var replayStore = new PersistenceStateStore();
        foreach (var entry in await _journal.ReadAllAsync())
        {
            ((IInternalEntityApplier)_descriptor).ApplyUpsert(replayStore, entry.Payload);
        }

        // The next allocation must skip past the replayed ids.
        Assert.Equal(3, _descriptor.AllocateNextKey(replayStore));
    }

    public async ValueTask DisposeAsync()
    {
        await _journal.DisposeAsync();

        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }
    }
}
