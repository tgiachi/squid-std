using SquidStd.Database.Interfaces.Seeding;
using SquidStd.Database.Interfaces.Services;

namespace SquidStd.Database.Internal;

/// <summary>
/// Adapts a seeding delegate to <see cref="IDatabaseSeeder" /> so
/// <c>RegisterDatabaseSeeder(Func{IDatabaseService,CancellationToken,ValueTask})</c> can record
/// it alongside class-based seeders.
/// </summary>
internal sealed class DelegateDatabaseSeeder : IDatabaseSeeder
{
    private readonly Func<IDatabaseService, CancellationToken, ValueTask> _seed;

    public string Name { get; }

    public DelegateDatabaseSeeder(string name, Func<IDatabaseService, CancellationToken, ValueTask> seed)
    {
        Name = name;
        _seed = seed;
    }

    public ValueTask SeedAsync(IDatabaseService database, CancellationToken cancellationToken = default)
        => _seed(database, cancellationToken);
}
