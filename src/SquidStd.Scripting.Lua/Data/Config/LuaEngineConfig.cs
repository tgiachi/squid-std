namespace SquidStd.Scripting.Lua.Data.Config;

public sealed record LuaEngineConfig
{
    public string LuarcDirectory { get; }
    public string ScriptsDirectory { get; }
    public string EngineVersion { get; }

    public LuaEngineConfig(string luarcDirectory, string scriptsDirectory, string engineVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(luarcDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptsDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(engineVersion);

        LuarcDirectory = luarcDirectory;
        ScriptsDirectory = scriptsDirectory;
        EngineVersion = engineVersion;
    }
}
