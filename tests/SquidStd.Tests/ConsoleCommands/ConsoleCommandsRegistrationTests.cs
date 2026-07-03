using DryIoc;
using Serilog.Core;
using SquidStd.ConsoleCommands.Extensions;
using SquidStd.ConsoleCommands.Interfaces;
using SquidStd.Core.Interfaces.Config;
using SquidStd.Services.Core.Extensions;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.ConsoleCommands;

public class ConsoleCommandsRegistrationTests
{
    [Fact]
    public void AddConsoleCommands_RegistersTheStack()
    {
        using var temp = new TempDirectory();
        using var container = new Container();
        container.RegisterConfigServices("app", temp.Path);

        container.AddConsoleCommands();
        container.Resolve<IConfigManagerService>().Load();

        Assert.True(container.IsRegistered<ICommandSystemService>());
        Assert.True(container.IsRegistered<IConsoleUiService>());
        Assert.True(container.IsRegistered<ILogEventSink>());
        Assert.NotNull(container.Resolve<ICommandSystemService>());
    }

    [Fact]
    public async Task BuiltinsAreRegistered()
    {
        using var temp = new TempDirectory();
        using var container = new Container();
        container.RegisterConfigServices("app", temp.Path);
        container.AddConsoleCommands();
        container.Resolve<IConfigManagerService>().Load();

        var system = container.Resolve<ICommandSystemService>();
        var output = await system.ExecuteCommandWithOutputAsync("help");

        Assert.Contains(output, line => line.Contains("clear", StringComparison.Ordinal));
    }
}
