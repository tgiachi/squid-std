using SquidStd.Plugin.Abstractions.Interfaces.Plugins;
using SquidStd.Plugin.Exceptions;

namespace SquidStd.Plugin.Internal;

/// <summary>
/// Orders plugins by dependency: a plugin always appears after every plugin it depends on.
/// Plugin ids are compared case-insensitively. Fails fast on duplicate ids, missing
/// dependencies, and dependency cycles.
/// </summary>
internal static class PluginDependencyResolver
{
    public static IReadOnlyList<ISquidStdPlugin> Sort(IReadOnlyList<ISquidStdPlugin> plugins)
    {
        var byId = new Dictionary<string, ISquidStdPlugin>(StringComparer.OrdinalIgnoreCase);

        foreach (var plugin in plugins)
        {
            if (!byId.TryAdd(plugin.Metadata.Id, plugin))
            {
                throw new PluginLoadException($"Duplicate plugin id '{plugin.Metadata.Id}'.");
            }
        }

        var sorted = new List<ISquidStdPlugin>(plugins.Count);
        var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var path = new List<string>();

        foreach (var plugin in plugins)
        {
            Visit(plugin, byId, visiting, visited, path, sorted);
        }

        return sorted;
    }

    private static void Visit(
        ISquidStdPlugin plugin,
        Dictionary<string, ISquidStdPlugin> byId,
        HashSet<string> visiting,
        HashSet<string> visited,
        List<string> path,
        List<ISquidStdPlugin> sorted
    )
    {
        var id = plugin.Metadata.Id;

        if (visited.Contains(id))
        {
            return;
        }

        if (!visiting.Add(id))
        {
            var start = path.FindIndex(p => string.Equals(p, id, StringComparison.OrdinalIgnoreCase));
            var cycle = string.Join(" -> ", path.Skip(start).Append(id));

            throw new PluginLoadException($"Plugin dependency cycle detected: {cycle}.");
        }

        path.Add(id);

        foreach (var dependencyId in plugin.Metadata.Dependencies)
        {
            if (!byId.TryGetValue(dependencyId, out var dependency))
            {
                throw new PluginLoadException(
                    $"Plugin '{id}' depends on '{dependencyId}', which is not registered."
                );
            }

            Visit(dependency, byId, visiting, visited, path, sorted);
        }

        path.RemoveAt(path.Count - 1);
        visiting.Remove(id);
        visited.Add(id);
        sorted.Add(plugin);
    }
}
