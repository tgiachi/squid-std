using DryIoc;
using SquidStd.Abstractions.Extensions.Services;
using SquidStd.Generators.Scripting.Lua;
using SquidStd.Scripting.Lua.Attributes;
using SquidStd.Scripting.Lua.Attributes.Scripts;
using SquidStd.Scripting.Lua.Data.Config;
using SquidStd.Scripting.Lua.Extensions.Scripts;
using SquidStd.Scripting.Lua.Interfaces.Scripts;
using SquidStd.Scripting.Lua.Modules;
using SquidStd.Scripting.Lua.Services;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Services.Core.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;

var bootstrap = SquidStdBootstrap.Create(
    new SquidStdOptions()
    {
        ConfigName = "squidstd",
        RootDirectory = AppContext.BaseDirectory
    }
);

#region step-1

var scriptsDirectory = Path.Combine(AppContext.BaseDirectory, "scripts");
Directory.CreateDirectory(scriptsDirectory);

bootstrap.ConfigureServices(
    container =>
    {
        container.RegisterCoreServices();

        var engineConfig = new LuaEngineConfig(
            AppContext.BaseDirectory,
            scriptsDirectory,
            "SquidStd.Samples.ScriptingLua",
            "1.0.0"
        );

        container.RegisterInstance(engineConfig);
        container.RegisterStdService<IScriptEngineService, LuaScriptEngineService>();
        container.RegisterGeneratedScriptModules();
        container.RegisterScriptModule<LogModule>();
        container.RegisterLuaEvents();

        return container;
    }
);

#endregion

await bootstrap.StartAsync();

#region step-2

var engine = bootstrap.Resolve<IScriptEngineService>();
var stats = ((LuaScriptEngineService)engine).GetStats();

engine.RegisterGlobal("greeting", "hello from C#");
engine.ExecuteScript("result = greeting .. ' and lua'");

var sum = engine.ExecuteFunction("3 + 4");
var message = engine.ExecuteFunction("result");

Console.WriteLine($"lua modules = {stats.ModuleCount}");
Console.WriteLine($"3 + 4 = {sum.Data}");
Console.WriteLine($"result = {message.Data}");

#endregion

engine.ExecuteScript(
    """
    events.subscribe("engine_stopped", function(e)
        log.info("lua saw engine_stopped for " .. e.application)
    end)
    """
);

await bootstrap.StopAsync();

#region step-3

[RegisterScriptModule, ScriptModule("sample")]
internal sealed class SampleLuaModule { }

#endregion
