using System.Reflection;
using Serilog;
using SquidStd.Plugin.Abstractions.Interfaces.Plugins;
using SquidStd.Plugin.Exceptions;

namespace SquidStd.Plugin.Internal;

/// <summary>
/// Scans a directory for external plugin assemblies, loads each into the default
/// AssemblyLoadContext, and instantiates every concrete <see cref="ISquidStdPlugin"/> found.
/// </summary>
internal static class PluginAssemblyScanner
{
    private static readonly ILogger Logger = Log.ForContext(typeof(PluginAssemblyScanner));

    /// <summary>
    /// Scans <paramref name="directory"/> for <c>*.dll</c> files and returns every concrete
    /// <see cref="ISquidStdPlugin"/> instantiated from them.
    /// </summary>
    /// <param name="directory">The directory to scan for plugin assemblies.</param>
    /// <exception cref="PluginLoadException">
    /// The directory does not exist, an assembly failed to load, or a plugin type failed to instantiate.
    /// </exception>
    public static IReadOnlyList<ISquidStdPlugin> Scan(string directory)
    {
        if (!Directory.Exists(directory))
        {
            throw new PluginLoadException($"Plugin directory '{directory}' does not exist.");
        }

        var plugins = new List<ISquidStdPlugin>();
        var dllPaths = Directory.EnumerateFiles(directory, "*.dll")
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);

        foreach (var dllPath in dllPaths)
        {
            List<Type> pluginTypes;

            try
            {
                // Default AssemblyLoadContext: plugins are trusted and share type identity
                // with the host (ISquidStdPlugin, DryIoc). No unload, no version isolation.
                var assembly = Assembly.LoadFrom(dllPath);

                pluginTypes = assembly.GetTypes()
                    .Where(type => typeof(ISquidStdPlugin).IsAssignableFrom(type) &&
                                   type is { IsClass: true, IsAbstract: false })
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new PluginLoadException($"Failed to load plugin assembly '{dllPath}'.", ex);
            }

            if (pluginTypes.Count == 0)
            {
                Logger.Debug("No plugins found in {AssemblyPath:l}", dllPath);

                continue;
            }

            foreach (var pluginType in pluginTypes)
            {
                try
                {
                    plugins.Add((ISquidStdPlugin)Activator.CreateInstance(pluginType)!);
                }
                catch (Exception ex)
                {
                    throw new PluginLoadException(
                        $"Failed to instantiate plugin type '{pluginType.FullName}' from '{dllPath}'.",
                        ex
                    );
                }
            }
        }

        return plugins;
    }
}
