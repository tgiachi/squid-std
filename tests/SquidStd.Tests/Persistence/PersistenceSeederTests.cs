using DryIoc;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Persistence.Abstractions.Interfaces.Persistence;
using SquidStd.Persistence.Extensions;
using SquidStd.Services.Core.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Persistence;

public class PersistenceSeederTests
{
    public sealed class SeedEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    public sealed class GreetingProvider
    {
        public string Greeting { get; } = "seeded-by-class";
    }

    public sealed class ClassSeeder : IPersistenceSeeder
    {
        private readonly GreetingProvider _greetings;

        public ClassSeeder(GreetingProvider greetings)
        {
            _greetings = greetings;
        }

        public async ValueTask SeedAsync(IPersistenceService persistence, CancellationToken _ = default)
            => await persistence.GetStore<SeedEntity, int>()
                                .UpsertAsync(new() { Id = 2, Name = _greetings.Greeting });
    }

    private static SquidStdBootstrap CreateBootstrap(
        TempDirectory root,
        Action<IContainer>? extra = null
    )
    {
        var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "seedtest", RootDirectory = root.Path }
        );

        bootstrap.ConfigureServices(c =>
        {
            c.RegisterDataSerializer();
            c.RegisterPersistence();
            c.RegisterPersistedEntity<SeedEntity, int>(1, "SeedEntity", 1, entity => entity.Id);
            extra?.Invoke(c);

            return c;
        });

        return bootstrap;
    }

    [Fact]
    public async Task Seeder_RunsOnFreshSave_AndDataIsPresent()
    {
        using var root = new TempDirectory();
        var runs = 0;

        await using var bootstrap = CreateBootstrap(
            root,
            c => c.RegisterPersistenceSeeder(async (persistence, _) =>
            {
                runs++;
                await persistence.GetStore<SeedEntity, int>().UpsertAsync(new() { Id = 1, Name = "seeded" });
            })
        );

        await bootstrap.StartAsync();

        Assert.Equal(1, runs);
        var stored = await bootstrap.Resolve<IPersistenceService>().GetStore<SeedEntity, int>().GetByIdAsync(1);
        Assert.Equal("seeded", stored!.Name);
        await bootstrap.StopAsync();
    }

    [Fact]
    public async Task Seeder_DoesNotRerun_OnSecondBoot()
    {
        using var root = new TempDirectory();
        var runs = 0;

        Func<IPersistenceService, CancellationToken, ValueTask> seed = async (persistence, _) =>
        {
            runs++;
            await persistence.GetStore<SeedEntity, int>().UpsertAsync(new() { Id = 1, Name = "seeded" });
        };

        await using (var first = CreateBootstrap(root, c => c.RegisterPersistenceSeeder(seed)))
        {
            await first.StartAsync();
            await first.StopAsync();
        }

        await using var second = CreateBootstrap(root, c => c.RegisterPersistenceSeeder(seed));
        await second.StartAsync();

        Assert.Equal(1, runs); // journal from the first boot makes the save non-fresh
        await second.StopAsync();
    }

    [Fact]
    public async Task Seeder_DoesNotRun_WhenSaveHasPriorData()
    {
        using var root = new TempDirectory();

        await using (var first = CreateBootstrap(root))
        {
            await first.StartAsync();
            await first.Resolve<IPersistenceService>().GetStore<SeedEntity, int>()
                       .UpsertAsync(new() { Id = 9, Name = "existing" });
            await first.StopAsync();
        }

        var runs = 0;

        await using var second = CreateBootstrap(
            root,
            c => c.RegisterPersistenceSeeder((_, _) =>
            {
                runs++;

                return ValueTask.CompletedTask;
            })
        );
        await second.StartAsync();

        Assert.Equal(0, runs);
        await second.StopAsync();
    }

    [Fact]
    public async Task ClassSeeder_ResolvesDependencies()
    {
        using var root = new TempDirectory();

        await using var bootstrap = CreateBootstrap(
            root,
            c =>
            {
                c.RegisterInstance(new GreetingProvider());
                c.RegisterPersistenceSeeder<ClassSeeder>();
            }
        );

        await bootstrap.StartAsync();

        var stored = await bootstrap.Resolve<IPersistenceService>().GetStore<SeedEntity, int>().GetByIdAsync(2);
        Assert.Equal("seeded-by-class", stored!.Name);
        await bootstrap.StopAsync();
    }

    [Fact]
    public async Task Seeders_RunInRegistrationOrder()
    {
        using var root = new TempDirectory();
        var order = new List<string>();

        await using var bootstrap = CreateBootstrap(
            root,
            c =>
            {
                c.RegisterPersistenceSeeder((_, _) =>
                {
                    order.Add("first");

                    return ValueTask.CompletedTask;
                });
                c.RegisterPersistenceSeeder((_, _) =>
                {
                    order.Add("second");

                    return ValueTask.CompletedTask;
                });
            }
        );

        await bootstrap.StartAsync();

        Assert.Equal(["first", "second"], order);
        await bootstrap.StopAsync();
    }

    [Fact]
    public async Task SeederException_FailsStartup()
    {
        using var root = new TempDirectory();

        await using var bootstrap = CreateBootstrap(
            root,
            c => c.RegisterPersistenceSeeder((_, _) => throw new InvalidOperationException("seed boom"))
        );

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await bootstrap.StartAsync());
    }
}
