using DryIoc;
using Microsoft.AspNetCore.Builder;
using Serilog;
using SquidStd.Core.Interfaces.Bootstrap;

namespace SquidStd.AspNetCore.Extensions;

/// <summary>
/// Extension methods that route ASP.NET Core and SquidStd logging through a single Serilog pipeline.
/// </summary>
public static class SquidStdSerilogExtensions
{
    /// <param name="builder">ASP.NET Core application builder.</param>
    extension(WebApplicationBuilder builder)
    {
        /// <summary>
        /// Configures SquidStd's Serilog logger eagerly (from the SquidStd YAML configuration) and routes
        /// ASP.NET Core framework logging through it, so framework and SquidStd logs share one config and
        /// one format. Must be called after <c>UseSquidStd</c> and before <c>Build</c>.
        /// </summary>
        /// <returns>The same builder for chaining.</returns>
        public WebApplicationBuilder AddSquidStdSerilog()
        {
            ArgumentNullException.ThrowIfNull(builder);

            if (!builder.Host.Properties.TryGetValue(
                    SquidStdAspNetCoreBuilderExtensions.ContainerPropertyKey,
                    out var value) || value is not IContainer container)
            {
                throw new InvalidOperationException("AddSquidStdSerilog must be called after UseSquidStd.");
            }

            var bootstrap = container.Resolve<ISquidStdBootstrap>();
            bootstrap.ConfigureLogging();

            builder.Host.UseSerilog(Log.Logger, dispose: false);

            return builder;
        }
    }
}
