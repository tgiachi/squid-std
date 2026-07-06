using DryIoc;
using SquidStd.Abstractions.Extensions.Config;
using SquidStd.Core.Data.Config;
using SquidStd.Core.Interfaces.Health;
using SquidStd.Services.Core.Services;

namespace SquidStd.Services.Core.Extensions;

/// <summary>
/// Extension methods for registering the health-check aggregator.
/// </summary>
public static class RegisterHealthChecksServiceExtension
{
    /// <param name="container">Container that receives the health-check registration.</param>
    extension(IContainer container)
    {
        /// <summary>
        /// Registers the health-check aggregator (<see cref="IHealthCheckService" />) as a singleton.
        /// Concrete checks register themselves as <c>IHealthCheck</c> and are collected automatically.
        /// </summary>
        /// <param name="config">Explicit configuration; when set, the YAML section is not bound and the file is ignored for this section.</param>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterHealthChecksService(HealthCheckOptions? config = null)
        {
            if (config is not null)
            {
                container.RegisterInstance(config, IfAlreadyRegistered.Replace);
            }
            else
            {
                container.RegisterConfigSection("healthChecks", static () => new HealthCheckOptions(), -80);
            }

            container.Register<IHealthCheckService, HealthCheckService>(Reuse.Singleton);

            return container;
        }
    }
}
