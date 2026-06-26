using SquidStd.Persistence.Abstractions.Data;
using SquidStd.Persistence.Abstractions.Interfaces.Persistence;
using SquidStd.Persistence.Data;
using SquidStd.Persistence.MessagePack;
using SquidStd.Persistence.Services;

// A standalone demo of SquidStd.Persistence: an in-memory entity store backed by a durable
// binary snapshot + journal. Run it twice — the second run reloads the state saved by the first.

var saveDir = Path.Combine(AppContext.BaseDirectory, "save");

IPersistenceService BuildPersistence()
{
    var serializer = new MessagePackDataSerializer();

    var registry = new PersistenceEntityRegistry();
    registry.Register(
        new PersistenceEntityDescriptor<Player, int>(
            serializer,
            serializer,
            typeId: 1,
            typeName: "Player",
            schemaVersion: 1,
            keySelector: player => player.Id
        )
    );

    var config = new PersistenceConfig { SaveDirectory = saveDir, AutosaveInterval = TimeSpan.FromMinutes(5) };
    var journal = new BinaryJournalService(Path.Combine(saveDir, config.JournalFileName));
    var snapshot = new SnapshotService(saveDir, config.SnapshotFileSuffix);

    return new PersistenceService(registry, journal, snapshot, config);
}

#region step-1: load existing state (snapshot + journal replay)

var persistence = BuildPersistence();
await persistence.InitializeAsync();

var players = persistence.GetStore<Player, int>();

Console.WriteLine($"Loaded {await players.CountAsync()} player(s) from {saveDir}");

foreach (var existing in await players.GetAllAsync())
{
    Console.WriteLine($"  - #{existing.Id} {existing.Name} (level {existing.Level})");
}

#endregion

#region step-2: mutate — every upsert/remove is appended to the journal

var nextId = (await players.CountAsync()) + 1;
await players.UpsertAsync(new Player { Id = nextId, Name = $"Hero-{nextId}", Level = nextId * 10 });

Console.WriteLine($"Added player #{nextId}; store now holds {await players.CountAsync()} player(s)");

#endregion

#region step-3: snapshot — capture full state and trim the journal

await persistence.SaveSnapshotAsync();
Console.WriteLine("Snapshot saved. Re-run this sample to see the state reload.");

#endregion

public sealed class Player
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
}
