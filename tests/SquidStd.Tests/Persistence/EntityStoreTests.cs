using SquidStd.Core.Json;
using SquidStd.Persistence.Abstractions.Interfaces.Persistence;
using SquidStd.Persistence.Data;
using SquidStd.Persistence.Internal;
using SquidStd.Persistence.Services;

namespace SquidStd.Tests.Persistence;

public sealed class EntityStoreTests : IAsyncDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "squidstd-store-" + Guid.NewGuid().ToString("N"));
    private readonly BinaryJournalService _journal;
    private readonly PersistenceStateStore _stateStore = new();
    private readonly EntityStore<Player, int> _store;

    public EntityStoreTests()
    {
        var serializer = new JsonDataSerializer();
        IPersistenceEntityDescriptor<Player, int> descriptor =
            new PersistenceEntityDescriptor<Player, int>(serializer, serializer, 1, "Player", 1, p => p.Id);
        _journal = new(Path.Combine(_dir, "world.journal.bin"));
        _store = new(_stateStore, _journal, descriptor);
    }

    [Fact]
    public async Task Upsert_ThenGetById_ReturnsClone()
    {
        await _store.UpsertAsync(new() { Id = 1, Name = "Bob" });

        var fetched = await _store.GetByIdAsync(1);

        Assert.NotNull(fetched);
        Assert.Equal("Bob", fetched.Name);
    }

    [Fact]
    public async Task GetById_ReturnsDetachedClone()
    {
        await _store.UpsertAsync(new() { Id = 1, Tags = ["a"] });

        var first = await _store.GetByIdAsync(1);
        first!.Tags.Add("mutated");
        var second = await _store.GetByIdAsync(1);

        Assert.Equal(["a"], second!.Tags);
    }

    [Fact]
    public async Task Upsert_AppendsToJournal()
    {
        await _store.UpsertAsync(new() { Id = 1 });

        Assert.Single(await _journal.ReadAllAsync());
    }

    [Fact]
    public async Task Remove_ExistingKey_ReturnsTrueAndJournals()
    {
        await _store.UpsertAsync(new() { Id = 1 });

        Assert.True(await _store.RemoveAsync(1));
        Assert.Null(await _store.GetByIdAsync(1));
        Assert.Equal(2, (await _journal.ReadAllAsync()).Count);
    }

    [Fact]
    public async Task Remove_MissingKey_ReturnsFalseAndDoesNotJournal()
    {
        Assert.False(await _store.RemoveAsync(99));
        Assert.Empty(await _journal.ReadAllAsync());
    }

    [Fact]
    public async Task CountAndGetAll_ReflectState()
    {
        await _store.UpsertAsync(new() { Id = 1 });
        await _store.UpsertAsync(new() { Id = 2 });

        Assert.Equal(2, await _store.CountAsync());
        Assert.Equal(2, (await _store.GetAllAsync()).Count);
    }

    [Fact]
    public async Task Query_ReturnsQueryableClones()
    {
        await _store.UpsertAsync(new() { Id = 1, Name = "Alice" });
        await _store.UpsertAsync(new() { Id = 2, Name = "Bob" });

        var names = _store.Query().Where(p => p.Name.StartsWith('A')).Select(p => p.Name).ToArray();

        Assert.Equal(["Alice"], names);
    }

    [Fact]
    public async Task Count_And_GetAll_Sync_ReflectStateAfterUpserts()
    {
        await _store.UpsertAsync(new() { Id = 1 });
        await _store.UpsertAsync(new() { Id = 2 });

        Assert.Equal(2, _store.Count());
        Assert.Equal(2, _store.GetAll().Count);
    }

    [Fact]
    public async Task GetById_Sync_ReturnsDetachedClone()
    {
        await _store.UpsertAsync(new() { Id = 1, Tags = ["a"] });

        var first = _store.GetById(1);
        first!.Tags.Add("mutated");
        var second = _store.GetById(1);

        Assert.Equal(["a"], second!.Tags);
    }

    [Fact]
    public void GetById_Sync_MissingKey_ReturnsDefault()
    {
        Assert.Null(_store.GetById(99));
    }

    [Fact]
    public async Task SyncAndAsyncReads_Agree()
    {
        await _store.UpsertAsync(new() { Id = 1, Name = "Alice" });
        await _store.UpsertAsync(new() { Id = 2, Name = "Bob" });

        Assert.Equal(await _store.CountAsync(), _store.Count());
        Assert.Equal((await _store.GetAllAsync()).Count, _store.GetAll().Count);

        var asyncEntity = await _store.GetByIdAsync(1);
        var syncEntity = _store.GetById(1);

        Assert.Equal(asyncEntity!.Name, syncEntity!.Name);
    }

    public async ValueTask DisposeAsync()
    {
        await _journal.DisposeAsync();

        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }
    }

    private sealed class Player
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = [];
    }
}
