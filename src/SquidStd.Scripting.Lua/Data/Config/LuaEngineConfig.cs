namespace SquidStd.Scripting.Lua.Data.Config;

public sealed record LuaEngineConfig
{
    public LuaEngineConfig(string luarcDirectory, string scriptsDirectory, string engineName, string engineVersion)
    {
        EngineName = engineName;
        LuarcDirectory = luarcDirectory;
        ScriptsDirectory = scriptsDirectory;
        EngineVersion = engineVersion;
    }

    public string LuarcDirectory { get; }
    public string ScriptsDirectory { get; }
    public string EngineVersion { get; }

    public string EngineName { get; }
}
