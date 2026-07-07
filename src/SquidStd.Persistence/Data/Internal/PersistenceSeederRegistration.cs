using DryIoc;
using SquidStd.Persistence.Abstractions.Interfaces.Persistence;

namespace SquidStd.Persistence.Data.Internal;

/// <summary>
/// A declarative persistence-seeder registration; the resolver runs when the persistence
/// service is constructed.
/// </summary>
public sealed record PersistenceSeederRegistration(Func<IResolverContext, IPersistenceSeeder> Resolve);
