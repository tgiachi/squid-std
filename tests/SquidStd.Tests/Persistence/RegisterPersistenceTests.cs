using SquidStd.Core.Data.Bootstrap;
using SquidStd.Persistence.Abstractions.Data;
using SquidStd.Persistence.Abstractions.Interfaces.Persistence;
using SquidStd.Persistence.Extensions;
using SquidStd.Persistence.MessagePack.Extensions;
using SquidStd.Services.Core.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Persistence;

public class RegisterPersistenceTests
{
    public sealed class PlayerEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    private static SquidStdBootstrap CreateBootstrap(TempDirectory root, PersistenceConfig? config = null)
    {
        var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "persist", RootDirectory = root.Path }
        );

        bootstrap.ConfigureServices(c =>
        {
            c.RegisterDataSerializer();
            c.RegisterPersistence(config);
            c.RegisterPersistedEntity<PlayerEntity, int>(1, "Player", 1, player => player.Id);

            return c;
        });

        return bootstrap;
    }

    [Fact]
    public async Task RegisterPersistence_RoundTrip_PersistsAcrossBootstraps()
    {
        using var root = new TempDirectory();

        await using (var first = CreateBootstrap(root))
        {
            await first.StartAsync();

            var store = first.Resolve<IPersistenceService>().GetStore<PlayerEntity, int>();
            await store.UpsertAsync(new() { Id = 1, Name = "Hero" });

            await first.StopAsync();
        }

        await using var second = CreateBootstrap(root);
        await second.StartAsync();

        var reloaded = second.Resolve<IPersistenceService>().GetStore<PlayerEntity, int>();
        var hero = await reloaded.GetByIdAsync(1);

        Assert.NotNull(hero);
        Assert.Equal("Hero", hero!.Name);
        await second.StopAsync();
    }

    [Fact]
    public async Task RegisterPersistence_DefaultsSaveDirectoryToManagedSaveDir()
    {
        using var root = new TempDirectory();

        await using var bootstrap = CreateBootstrap(root);
        await bootstrap.StartAsync();

        // CaptureBucket returns null for an empty bucket (no snapshot file is written for it), so
        // an entity must be upserted for the final snapshot to actually land on disk.
        var store = bootstrap.Resolve<IPersistenceService>().GetStore<PlayerEntity, int>();
        await store.UpsertAsync(new() { Id = 1, Name = "Hero" });

        await bootstrap.StopAsync(); // final snapshot lands in the save dir

        Assert.True(Directory.Exists(root.Combine("save")));
        Assert.NotEmpty(Directory.GetFiles(root.Combine("save")));
    }

    [Fact]
    public async Task RegisterPersistence_ExplicitConfigInstance_Survives()
    {
        using var root = new TempDirectory();
        var explicitConfig = new PersistenceConfig { SaveDirectory = root.Combine("custom_save") };

        await using var bootstrap = CreateBootstrap(root, explicitConfig);
        await bootstrap.StartAsync();

        Assert.Same(explicitConfig, bootstrap.Resolve<PersistenceConfig>());
        await bootstrap.StopAsync();

        Assert.True(Directory.Exists(root.Combine("custom_save")));
    }

    [Fact]
    public void RegisterPersistence_WithoutSerializer_Throws()
    {
        using var root = new TempDirectory();
        var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "persist", RootDirectory = root.Path }
        );

        var ex = Assert.Throws<InvalidOperationException>(
            () => bootstrap.ConfigureServices(c => c.RegisterPersistence())
        );

        Assert.Contains("serializer", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RegisterMessagePackSerializer_RegistersBothInterfaces()
    {
        using var root = new TempDirectory();
        var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "persist", RootDirectory = root.Path }
        );

        bootstrap.ConfigureServices(c => c.RegisterMessagePackSerializer());

        Assert.Same(
            bootstrap.Resolve<SquidStd.Core.Interfaces.Serialization.IDataSerializer>(),
            bootstrap.Resolve<SquidStd.Core.Interfaces.Serialization.IDataDeserializer>()
        );
    }
}
