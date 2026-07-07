using SquidStd.Database.Interfaces.Services;

namespace SquidStd.Database.Interfaces.Seeding;

/// <summary>
/// Seeds initial database data. Each seeder runs once ever per unique <see cref="Name" />,
/// tracked in the __squidstd_seed_history table; the history row is written only after a
/// successful run, so a failed seeder retries at the next start. A seeder exception aborts
/// startup.
/// </summary>
public interface IDatabaseSeeder
{
    /// <summary>Unique, stable seeder name recorded in the history table.</summary>
    string Name { get; }

    /// <summary>Writes the seed data through the ORM.</summary>
    /// <param name="database">The started database service.</param>
    /// <param name="cancellationToken">Token used to cancel the seeding.</param>
    ValueTask SeedAsync(IDatabaseService database, CancellationToken cancellationToken = default);
}
