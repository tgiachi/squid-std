using DryIoc;
using SquidStd.Abstractions.Extensions.Config;
using SquidStd.Abstractions.Extensions.Services;
using SquidStd.Database.Abstractions.Data.Database;
using SquidStd.Database.Abstractions.Interfaces.Data;
using SquidStd.Database.Data;
using SquidStd.Database.Interfaces.Services;
using SquidStd.Database.Services;

namespace SquidStd.Database.Extensions;

/// <summary>
///     DI registration for the database subsystem.
/// </summary>
public static class RegisterDatabaseExtension
{
    /// <param name="container">The DI container.</param>
    extension(IContainer container)
    {
        /// <summary>
        ///     Registers the database config section, the database service, and the open-generic data access.
        /// </summary>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterDatabase()
        {
            container.RegisterConfigSection<DatabaseConfig>("database");
            container.RegisterStdService<IDatabaseService, DatabaseService>();
            container.Register(typeof(IDataAccess<>), typeof(FreeSqlDataAccess<>), Reuse.Transient);

            return container;
        }
    }
}
