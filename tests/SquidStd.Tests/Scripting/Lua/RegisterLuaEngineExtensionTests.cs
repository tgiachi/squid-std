using DryIoc;
using SquidStd.Scripting.Lua.Data.Config;
using SquidStd.Scripting.Lua.Extensions.Scripts;
using SquidStd.Scripting.Lua.Interfaces.Scripts;

namespace SquidStd.Tests.Scripting.Lua;

public class RegisterLuaEngineExtensionTests
{
    [Fact]
    public void RegisterLuaEngine_RegistersConfigAndService()
    {
        using var container = new Container();
        var config = new LuaEngineConfig(AppContext.BaseDirectory, AppContext.BaseDirectory, "tests", "1.0.0");

        container.RegisterLuaEngine(config);

        Assert.Same(config, container.Resolve<LuaEngineConfig>());
        Assert.True(container.IsRegistered<IScriptEngineService>());
    }

    [Fact]
    public void RegisterLuaEngine_NullConfig_Throws()
        => Assert.Throws<ArgumentNullException>(() => new Container().RegisterLuaEngine(null!));
}
