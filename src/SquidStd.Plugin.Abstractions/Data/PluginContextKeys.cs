namespace SquidStd.Plugin.Abstractions.Data;

/// <summary>
/// Well-known keys the SquidStd plugin loader populates in <see cref="PluginContext.Data" />.
/// </summary>
public static class PluginContextKeys
{
    /// <summary>
    /// Application root directory (string): the bootstrap RootDirectory option.
    /// </summary>
    public const string RootDirectory = "squidstd:rootDirectory";

    /// <summary>
    /// Application name (string): the bootstrap AppName option when set, otherwise the entry
    /// assembly name, otherwise the bootstrap ConfigName.
    /// </summary>
    public const string AppName = "squidstd:appName";
}
