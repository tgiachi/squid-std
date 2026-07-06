using SquidStd.Plugin.Abstractions.Interfaces.Plugins;

namespace SquidStd.Plugin;

/// <summary>
/// Collects internal plugin registrations and external plugin directories for the loader.
/// Collection only: nothing is loaded or validated until the loader runs.
/// </summary>
public sealed class PluginCollectionBuilder
{
    private readonly List<ISquidStdPlugin> _plugins = [];
    private readonly List<string> _directories = [];

    internal IReadOnlyList<ISquidStdPlugin> Plugins => _plugins;

    internal IReadOnlyList<string> Directories => _directories;

    /// <summary>
    /// Registers an internal plugin by type; the loader instantiates it via the public
    /// parameterless constructor.
    /// </summary>
    /// <typeparam name="TPlugin">The plugin type.</typeparam>
    /// <returns>The same builder for chaining.</returns>
    public PluginCollectionBuilder Add<TPlugin>()
        where TPlugin : ISquidStdPlugin, new()
    {
        _plugins.Add(new TPlugin());

        return this;
    }

    /// <summary>
    /// Registers an internal plugin instance.
    /// </summary>
    /// <param name="plugin">The plugin instance.</param>
    /// <returns>The same builder for chaining.</returns>
    public PluginCollectionBuilder Add(ISquidStdPlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        _plugins.Add(plugin);

        return this;
    }

    /// <summary>
    /// Adds a directory to scan for external plugin assemblies (*.dll, non-recursive).
    /// Relative paths are resolved against the bootstrap root directory.
    /// </summary>
    /// <param name="path">Absolute or root-relative directory path.</param>
    /// <returns>The same builder for chaining.</returns>
    public PluginCollectionBuilder FromDirectory(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _directories.Add(path);

        return this;
    }
}
