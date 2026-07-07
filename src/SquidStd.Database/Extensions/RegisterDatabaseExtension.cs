using DryIoc;
using SquidStd.Abstractions.Extensions.Config;
using SquidStd.Abstractions.Extensions.Container;
using SquidStd.Abstractions.Extensions.Services;
using SquidStd.Database.Abstractions.Data.Database;
using SquidStd.Database.Abstractions.Interfaces.Data;
using SquidStd.Database.Data;
using SquidStd.Database.Data.Internal;
using SquidStd.Database.Interfaces.Seeding;
using SquidStd.Database.Interfaces.Services;
using SquidStd.Database.Internal;
using SquidStd.Database.Services;

namespace SquidStd.Database.Extensions;

/// <summary>
/// DI registration for the database subsystem.
/// </summary>
public static class RegisterDatabaseExtension
{
    /// <param name="container">The DI container.</param>
    extension(IContainer container)
    {
        /// <summary>
        /// Registers a database seeder by type; it is resolved from the container when the
        /// database service is constructed and runs once ever per its Name.
        /// </summary>
        /// <typeparam name="TSeeder">The seeder type.</typeparam>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterDatabaseSeeder<TSeeder>()
            where TSeeder : class, IDatabaseSeeder
        {
            container.Register<TSeeder>(Reuse.Singleton);
            container.AddToRegisterTypedList(
                new DatabaseSeederRegistration(static resolver => resolver.Resolve<TSeeder>())
            );

            return container;
        }

        /// <summary>
        /// Registers a delegate-backed database seeder that runs once ever per
        /// <paramref name="name" />.
        /// </summary>
        /// <param name="name">Unique, stable seeder name recorded in the history table.</param>
        /// <param name="seed">The seeding callback.</param>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterDatabaseSeeder(string name, Func<IDatabaseService, CancellationToken, ValueTask> seed)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(seed);
            container.AddToRegisterTypedList(
                new DatabaseSeederRegistration(_ => new DelegateDatabaseSeeder(name, seed))
            );

            return container;
        }

        /// <summary>
        /// Registers the database config section, the database service, and the open-generic data access.
        /// </summary>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterDatabase()
        {
            container.RegisterConfigSection<DatabaseConfig>("database");
            container.RegisterStdService<IDatabaseService, DatabaseService>();
            container.Register(typeof(IDataAccess<>), typeof(FreeSqlDataAccess<>), Reuse.Transient);

            container.RegisterDelegate<IReadOnlyList<IDatabaseSeeder>>(
                static resolver =>
                {
                    var recorded = resolver.Resolve<List<DatabaseSeederRegistration>>(IfUnresolved.ReturnDefault);

                    return recorded is null
                               ? []
                               : recorded.Select(registration => registration.Resolve(resolver)).ToList();
                },
                Reuse.Singleton
            );

            return container;
        }
    }
}
