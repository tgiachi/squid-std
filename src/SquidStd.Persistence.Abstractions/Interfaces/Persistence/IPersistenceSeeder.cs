namespace SquidStd.Persistence.Abstractions.Interfaces.Persistence;

/// <summary>
/// Seeds initial data into a fresh persistence store. Seeders run after the snapshot load and
/// journal replay, only when the save is brand new (no snapshot and no journal existed), in
/// registration order. Seed writes go through the normal entity stores, so subsequent boots
/// are no longer fresh and seeders never run again. A seeder exception aborts startup.
/// </summary>
public interface IPersistenceSeeder
{
    /// <summary>Writes the seed data through the normal entity stores.</summary>
    /// <param name="persistence">The started persistence service.</param>
    /// <param name="cancellationToken">Token used to cancel the seeding.</param>
    ValueTask SeedAsync(IPersistenceService persistence, CancellationToken cancellationToken = default);
}
