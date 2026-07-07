using DryIoc;
using SquidStd.Database.Interfaces.Seeding;

namespace SquidStd.Database.Data.Internal;

/// <summary>
/// A declarative database-seeder registration; the resolver runs when the database service is
/// constructed.
/// </summary>
public sealed record DatabaseSeederRegistration(Func<IResolverContext, IDatabaseSeeder> Resolve);
