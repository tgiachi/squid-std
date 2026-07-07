using DryIoc;
using FreeSql.DataAnnotations;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Core.Directories;
using SquidStd.Database.Abstractions.Data.Database;
using SquidStd.Database.Data.Internal;
using SquidStd.Database.Extensions;
using SquidStd.Database.Interfaces.Seeding;
using SquidStd.Database.Interfaces.Services;
using SquidStd.Database.Services;
using SquidStd.Services.Core.Services.Bootstrap;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Database;

public class DatabaseSeederTests
{
    public sealed class SeedAccount
    {
        [Column(IsIdentity = true)]
        public int Id { get; set; }

        public string Login { get; set; } = string.Empty;
    }

    public sealed class RecordingSeeder : IDatabaseSeeder
    {
        private readonly Func<IDatabaseService, CancellationToken, ValueTask> _seed;

        public string Name { get; }

        public RecordingSeeder(string name, Func<IDatabaseService, CancellationToken, ValueTask> seed)
        {
            Name = name;
            _seed = seed;
        }

        public ValueTask SeedAsync(IDatabaseService database, CancellationToken cancellationToken = default)
            => _seed(database, cancellationToken);
    }

    public sealed class NamedClassSeeder : IDatabaseSeeder
    {
        public string Name => "class.seeder";

        public async ValueTask SeedAsync(IDatabaseService database, CancellationToken cancellationToken = default)
            => await database.Orm.Insert(new SeedAccount { Login = "class-seeded" })
                             .ExecuteAffrowsAsync(cancellationToken);
    }

    [Fact]
    public async Task Seeder_RunsOnce_AndWritesHistory()
    {
        using var root = new TempDirectory();
        var runs = 0;

        IDatabaseSeeder seeder = new RecordingSeeder(
            "accounts.admin",
            async (database, cancellationToken) =>
            {
                runs++;
                await database.Orm.Insert(new SeedAccount { Login = "admin" }).ExecuteAffrowsAsync(cancellationToken);
            }
        );

        var service = CreateService(root, [seeder]);
        await service.StartAsync();

        Assert.Equal(1, runs);
        Assert.True(await service.Orm.Select<SeedAccount>().AnyAsync());
        Assert.Equal(1, await service.Orm.Select<SeedHistoryEntity>().CountAsync());

        await service.StopAsync();
    }

    [Fact]
    public async Task Seeder_Skipped_OnSecondStart()
    {
        using var root = new TempDirectory();
        var runs = 0;

        IDatabaseSeeder CreateSeeder()
            => new RecordingSeeder(
                "accounts.admin",
                (_, _) =>
                {
                    runs++;

                    return ValueTask.CompletedTask;
                }
            );

        var first = CreateService(root, [CreateSeeder()]);
        await first.StartAsync();
        await first.StopAsync();

        var second = CreateService(root, [CreateSeeder()]);
        await second.StartAsync();

        Assert.Equal(1, runs);

        await second.StopAsync();
    }

    [Fact]
    public async Task FailedSeeder_RerunsNextStart()
    {
        using var root = new TempDirectory();
        var runs = 0;
        var shouldFail = true;

        IDatabaseSeeder CreateSeeder()
            => new RecordingSeeder(
                "flaky",
                (_, _) =>
                {
                    runs++;

                    if (shouldFail)
                    {
                        shouldFail = false;

                        throw new InvalidOperationException("boom");
                    }

                    return ValueTask.CompletedTask;
                }
            );

        var first = CreateService(root, [CreateSeeder()]);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await first.StartAsync());
        await first.StopAsync();

        var second = CreateService(root, [CreateSeeder()]);
        await second.StartAsync();

        Assert.Equal(2, runs);

        await second.StopAsync();
    }

    [Fact]
    public async Task DuplicateNames_FailStartup()
    {
        using var root = new TempDirectory();
        var runs = 0;

        IDatabaseSeeder MakeDuplicate()
            => new RecordingSeeder(
                "dup",
                (_, _) =>
                {
                    runs++;

                    return ValueTask.CompletedTask;
                }
            );

        var service = CreateService(root, [MakeDuplicate(), MakeDuplicate()]);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.StartAsync());

        Assert.Contains("dup", exception.Message, StringComparison.Ordinal);
        Assert.Equal(0, runs);
    }

    [Fact]
    public async Task ClassSeeder_UsesItsName()
    {
        using var root = new TempDirectory();

        var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "dbseedtest", RootDirectory = root.Path }
        );

        bootstrap.ConfigureServices(c =>
        {
            c.RegisterInstance(
                new DatabaseConfig
                {
                    ConnectionString = $"sqlite://{root.Combine("class-seed.db")}",
                    AutoMigrate = true
                }
            );
            c.RegisterDatabase();
            c.RegisterDatabaseSeeder<NamedClassSeeder>();

            return c;
        });

        await bootstrap.StartAsync();

        var service = bootstrap.Resolve<IDatabaseService>();
        var applied = await service.Orm.Select<SeedHistoryEntity>()
                                    .Where(entry => entry.Name == "class.seeder")
                                    .AnyAsync();

        Assert.True(applied);
        Assert.True(await service.Orm.Select<SeedAccount>().AnyAsync());

        await bootstrap.StopAsync();
    }

    private static DatabaseService CreateService(TempDirectory root, IReadOnlyList<IDatabaseSeeder>? seeders)
        => new(
            new DatabaseConfig { ConnectionString = $"sqlite://{root.Combine("seed.db")}", AutoMigrate = true },
            new DirectoriesConfig(root.Path, []),
            seeders
        );
}
