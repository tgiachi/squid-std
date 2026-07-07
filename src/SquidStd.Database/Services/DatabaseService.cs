using FreeSql;
using Serilog;
using SquidStd.Database.Abstractions.Data.Database;
using SquidStd.Database.Abstractions.Types.Data;
using SquidStd.Core.Directories;
using SquidStd.Database.Connection;
using SquidStd.Database.Data.Internal;
using SquidStd.Database.Interfaces.Seeding;
using SquidStd.Database.Interfaces.Services;

namespace SquidStd.Database.Services;

/// <summary>
/// Builds and owns the singleton FreeSql instance, logging SQL and migrations verbosely.
/// </summary>
public sealed class DatabaseService : IDatabaseService
{
    private static readonly ILogger Logger = Log.ForContext<DatabaseService>();

    private readonly DatabaseConfig _config;
    private readonly DirectoriesConfig _directories;
    private readonly IReadOnlyList<IDatabaseSeeder> _seeders;
    private IFreeSql? _orm;
    private int _started;

    /// <inheritdoc />
    public IFreeSql Orm => _orm ?? throw new InvalidOperationException("Database service is not started.");

    /// <summary>
    /// Initializes the database service.
    /// </summary>
    /// <param name="config">The database configuration section.</param>
    /// <param name="directories">The directories configuration, providing the root directory.</param>
    /// <param name="seeders">
    /// Optional seeders run once ever, in order, right after the FreeSql instance is built in
    /// <see cref="StartAsync" />; applied names are tracked in the __squidstd_seed_history table.
    /// </param>
    public DatabaseService(DatabaseConfig config, DirectoriesConfig directories, IReadOnlyList<IDatabaseSeeder>? seeders = null)
    {
        _config = config;
        _directories = directories;
        _seeders = seeders ?? [];
    }

    /// <inheritdoc />
    public async ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (Interlocked.Exchange(ref _started, 1) != 0)
        {
            return;
        }

        var parsed = ConnectionStringParser.Parse(_config.ConnectionString, _directories.Root);

        if (parsed.SqliteFilePath is { } sqliteFile)
        {
            var directory = Path.GetDirectoryName(sqliteFile);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        Logger.Verbose("Building FreeSql for provider {Provider}", parsed.Provider);

        var builder = new FreeSqlBuilder()
                      .UseConnectionString(MapDataType(parsed.Provider), parsed.NativeConnectionString)
                      .UseAutoSyncStructure(_config.AutoMigrate)
                      .UseMonitorCommand(cmd => Logger.Verbose("SQL {Sql}", cmd.CommandText));

        _orm = builder.Build();
        _orm.Aop.SyncStructureAfter += (_, e) =>
                                           Logger.Verbose("Migrated {Entities} -> {Sql}", e.EntityTypes, e.Sql);

        if (_seeders.Count > 0)
        {
            await RunSeedersAsync(cancellationToken);
        }

        Logger.Information(
            "Database service started ({Provider}, autoMigrate={AutoMigrate})",
            parsed.Provider,
            _config.AutoMigrate
        );
    }

    private async ValueTask RunSeedersAsync(CancellationToken cancellationToken)
    {
        var duplicate = _seeders.GroupBy(seeder => seeder.Name, StringComparer.Ordinal)
                                .FirstOrDefault(group => group.Count() > 1);

        if (duplicate is not null)
        {
            throw new InvalidOperationException($"Duplicate database seeder name '{duplicate.Key}'.");
        }

        // The history table must exist even when AutoMigrate is off.
        Orm.CodeFirst.SyncStructure<SeedHistoryEntity>();

        foreach (var seeder in _seeders)
        {
            var applied = await Orm.Select<SeedHistoryEntity>()
                                   .Where(entry => entry.Name == seeder.Name)
                                   .AnyAsync(cancellationToken);

            if (applied)
            {
                Logger.Debug("Database seeder {Seeder:l} already applied; skipping", seeder.Name);

                continue;
            }

            await seeder.SeedAsync(this, cancellationToken);
            await Orm.Insert(new SeedHistoryEntity { Name = seeder.Name, AppliedAt = DateTime.UtcNow })
                     .ExecuteAffrowsAsync(cancellationToken);
            Logger.Information("Database seeder {Seeder:l} applied", seeder.Name);
        }
    }

    /// <inheritdoc />
    public ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _orm?.Dispose();
        _orm = null;

        return ValueTask.CompletedTask;
    }

    private static DataType MapDataType(DatabaseProviderType provider)
        => provider switch
        {
            DatabaseProviderType.Sqlite    => DataType.Sqlite,
            DatabaseProviderType.Postgres  => DataType.PostgreSQL,
            DatabaseProviderType.SqlServer => DataType.SqlServer,
            DatabaseProviderType.MySql     => DataType.MySql,
            _                              => throw new NotSupportedException($"Unsupported provider {provider}.")
        };
}
