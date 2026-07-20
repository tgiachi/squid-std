namespace SquidStd.Core.Config;

/// <summary>
/// Tracks the per-file <see cref="SquidStdConfig" />s bound via <c>RegisterConfigFile</c>, keyed by
/// resolved path so multiple sections in one file share one loader. The primary config is not held
/// here — only external files. The config manager iterates these for Save/Load/EnsureFiles.
/// </summary>
public sealed class ExternalConfigRegistry
{
    private readonly Dictionary<string, SquidStdConfig> _byPath = new(StringComparer.Ordinal);
    private readonly Lock _sync = new();

    /// <summary>The tracked external configs.</summary>
    public IReadOnlyCollection<SquidStdConfig> All
    {
        get
        {
            lock (_sync)
            {
                return _byPath.Values.ToArray();
            }
        }
    }

    /// <summary>
    /// Returns the config already tracked for <paramref name="path" />, or adds the one produced by
    /// <paramref name="factory" />.
    /// </summary>
    public SquidStdConfig GetOrAdd(string path, Func<SquidStdConfig> factory)
    {
        lock (_sync)
        {
            if (_byPath.TryGetValue(path, out var existing))
            {
                return existing;
            }

            var created = factory();
            _byPath[path] = created;

            return created;
        }
    }
}
