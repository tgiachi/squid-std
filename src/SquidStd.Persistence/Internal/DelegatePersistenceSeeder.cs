using SquidStd.Persistence.Abstractions.Interfaces.Persistence;

namespace SquidStd.Persistence.Internal;

/// <summary>
/// Adapts a seeding delegate to <see cref="IPersistenceSeeder" /> so
/// <c>RegisterPersistenceSeeder(Func{IPersistenceService,CancellationToken,ValueTask})</c> can
/// record it alongside class-based seeders.
/// </summary>
internal sealed class DelegatePersistenceSeeder : IPersistenceSeeder
{
    private readonly Func<IPersistenceService, CancellationToken, ValueTask> _seed;

    public DelegatePersistenceSeeder(Func<IPersistenceService, CancellationToken, ValueTask> seed)
    {
        _seed = seed;
    }

    public ValueTask SeedAsync(IPersistenceService persistence, CancellationToken cancellationToken = default)
        => _seed(persistence, cancellationToken);
}
