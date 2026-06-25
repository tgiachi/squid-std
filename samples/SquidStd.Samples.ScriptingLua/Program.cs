using DryIoc;
using SquidStd.Abstractions.Extensions.Services;
using SquidStd.Scripting.Lua.Data.Config;
using SquidStd.Scripting.Lua.Interfaces.Scripts;
using SquidStd.Scripting.Lua.Services;
using SquidStd.Services.Core.Services.Bootstrap;

var bootstrap = SquidStdBootstrap.Create(
    new()
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
        var engineConfig = new LuaEngineConfig(
            AppContext.BaseDirectory,
            scriptsDirectory,
            "SquidStd.Samples.ScriptingLua",
            "1.0.0"
        );

        container.RegisterInstance(engineConfig);
        container.RegisterStdService<IScriptEngineService, LuaScriptEngineService>();

        return container;
    }
);

#endregion

await bootstrap.StartAsync();

#region step-2

var engine = bootstrap.Resolve<IScriptEngineService>();

engine.RegisterGlobal("greeting", "hello from C#");
engine.ExecuteScript("result = greeting .. ' and lua'");

var sum = engine.ExecuteFunction("3 + 4");
var message = engine.ExecuteFunction("result");

Console.WriteLine($"3 + 4 = {sum.Data}");
Console.WriteLine($"result = {message.Data}");

#endregion

await bootstrap.StopAsync();
