using System.Reflection;
using Serilog;
using SquidStd.Core.Interfaces.Bootstrap;
using SquidStd.Core.Types.Bootstrap;
using SquidStd.Plugin.Abstractions.Data;
using SquidStd.Plugin.Abstractions.Interfaces.Plugins;
using SquidStd.Plugin.Internal;

namespace SquidStd.Plugin.Extensions;

/// <summary>
/// Connects the plugin loader to the SquidStd bootstrap.
/// </summary>
public static class SquidStdBootstrapPluginExtensions
{
    /// <summary>
    /// Collects internal plugins and external plugin directories, resolves the dependency order
    /// across the whole set, and invokes <see cref="ISquidStdPlugin.Configure" /> for each plugin
    /// in order against the bootstrap container. Must be called before the bootstrap starts, so
    /// plugins can register configuration sections before the configuration is loaded. Relative
    /// directories are resolved against the bootstrap root directory. Any failure aborts startup:
    /// loader problems raise <see cref="Exceptions.PluginLoadException" /> and plugin exceptions
    /// propagate unchanged.
    /// </summary>
    /// <param name="bootstrap">The bootstrap to configure.</param>
    /// <param name="configure">Callback that registers plugins and directories.</param>
    /// <returns>The same bootstrap for chaining.</returns>
    public static ISquidStdBootstrap UsePlugins(
        this ISquidStdBootstrap bootstrap,
        Action<PluginCollectionBuilder> configure
    )
    {
        ArgumentNullException.ThrowIfNull(bootstrap);
        ArgumentNullException.ThrowIfNull(configure);

        if (bootstrap.State != BootstrapStateType.Created)
        {
            throw new InvalidOperationException(
                "Plugins must be registered before the bootstrap starts."
            );
        }

        var logger = Log.ForContext(typeof(SquidStdBootstrapPluginExtensions));
        var builder = new PluginCollectionBuilder();

        configure(builder);

        var plugins = new List<ISquidStdPlugin>(builder.Plugins);

        foreach (var directory in builder.Directories)
        {
            var resolved = Path.IsPathRooted(directory)
                ? directory
                : Path.Combine(bootstrap.Options.RootDirectory, directory);

            plugins.AddRange(PluginAssemblyScanner.Scan(resolved));
        }

        var ordered = PluginDependencyResolver.Sort(plugins);
        var appName = bootstrap.Options.AppName
                      ?? Assembly.GetEntryAssembly()?.GetName().Name
                      ?? bootstrap.Options.ConfigName;

        foreach (var plugin in ordered)
        {
            var context = new PluginContext();
            context.Data[PluginContextKeys.RootDirectory] = bootstrap.Options.RootDirectory;
            context.Data[PluginContextKeys.AppName] = appName;

            plugin.Configure(bootstrap.Container, context);

            logger.Information(
                "Loaded plugin {PluginId:l} v{PluginVersion} by {PluginAuthor:l}",
                plugin.Metadata.Id,
                plugin.Metadata.Version,
                plugin.Metadata.Author
            );
        }

        logger.Information("{PluginCount} plugin(s) loaded", ordered.Count);

        return bootstrap;
    }
}
