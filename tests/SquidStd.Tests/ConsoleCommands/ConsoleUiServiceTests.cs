using Serilog.Events;
using SquidStd.ConsoleCommands.Data.Config;
using SquidStd.ConsoleCommands.Services;

namespace SquidStd.Tests.ConsoleCommands;

public class ConsoleUiServiceTests
{
    [Fact]
    public void NonInteractive_AllOperations_DoNotThrow()
    {
        var ui = new ConsoleUiService(new ConsoleCommandsConfig());

        Assert.False(ui.IsInteractive);
        ui.WriteLine("plain");
        ui.WriteLogLine("log line", LogEventLevel.Information);
        ui.LockInput();
        Assert.True(ui.IsInputLocked);
        ui.UnlockInput();
        Assert.False(ui.IsInputLocked);
        ui.UpdateInput("abc");
    }

    [Fact]
    public async Task InputLoop_NonInteractive_StartsAsNoOp()
    {
        var ui = new ConsoleUiService(new ConsoleCommandsConfig());
        var system = new CommandSystemService(_ => { });
        using var loop = new ConsoleInputLoopService(ui, system, new ConsoleCommandsConfig());

        await loop.StartAsync();
        await loop.StopAsync();
    }
}
