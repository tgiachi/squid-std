using SquidStd.ConsoleCommands.Services;

namespace SquidStd.Tests.ConsoleCommands;

public class CommandSystemServiceTests
{
    [Fact]
    public async Task RegisterAndExecute_InvokesHandlerWithArguments()
    {
        var uiLines = new List<string>();
        var service = new CommandSystemService(uiLines.Add);
        IReadOnlyList<string>? capturedArguments = null;

        service.RegisterCommand(
            "greet",
            ctx =>
            {
                capturedArguments = ctx.Arguments;
                ctx.WriteLine("hi " + ctx.Arguments[0]);

                return Task.CompletedTask;
            }
        );

        await service.ExecuteCommandAsync("greet \"squid dev\"");

        Assert.Equal(new[] { "squid dev" }, capturedArguments);
        Assert.Contains(uiLines, line => line.Contains("hi squid dev"));
    }

    [Fact]
    public async Task Aliases_ShareTheSameHandler()
    {
        var uiLines = new List<string>();
        var service = new CommandSystemService(uiLines.Add);
        var counter = 0;

        service.RegisterCommand(
            "gc|collect",
            _ =>
            {
                counter++;

                return Task.CompletedTask;
            },
            "run GC"
        );

        await service.ExecuteCommandAsync("gc");
        await service.ExecuteCommandAsync("COLLECT");

        Assert.Equal(2, counter);

        var definitions = service.GetRegisteredCommands();

        var definition = Assert.Single(definitions);
        Assert.Equal("gc", definition.Name);
        Assert.Contains("collect", definition.Aliases);
    }

    [Fact]
    public async Task UnknownCommand_WritesMessage_DoesNotThrow()
    {
        var uiLines = new List<string>();
        var service = new CommandSystemService(uiLines.Add);

        await service.ExecuteCommandAsync("nope");

        Assert.Contains(uiLines, line => line.Contains("Unknown command") && line.Contains("nope"));
    }

    [Fact]
    public async Task HandlerException_IsCaught_AndReported()
    {
        var uiLines = new List<string>();
        var service = new CommandSystemService(uiLines.Add);

        service.RegisterCommand("boom", _ => throw new InvalidOperationException("kaboom"));

        await service.ExecuteCommandAsync("boom");

        Assert.Contains(uiLines, line => line.Contains("kaboom") || line.Contains("failed"));
    }

    [Fact]
    public async Task ExecuteWithOutput_CapturesLines_WithoutWritingToUi()
    {
        var uiLines = new List<string>();
        var service = new CommandSystemService(uiLines.Add);

        service.RegisterCommand(
            "dump",
            ctx =>
            {
                ctx.WriteLine("a");
                ctx.WriteLine("b");

                return Task.CompletedTask;
            }
        );

        var output = await service.ExecuteCommandWithOutputAsync("dump");

        Assert.Equal(new[] { "a", "b" }, output);
        Assert.DoesNotContain(uiLines, line => line.Contains('a'));
    }

    [Fact]
    public void Autocomplete_CompletesCommandPrefix_AndUsesProviderForArgs()
    {
        var service = new CommandSystemService(_ => { });

        service.RegisterCommand("status", _ => Task.CompletedTask);
        service.RegisterCommand("stop", _ => Task.CompletedTask);
        service.RegisterCommand(
            "deploy",
            _ => Task.CompletedTask,
            autocompleteProvider: line => new[] { "deploy fast", "deploy slow" }
        );

        var commandSuggestions = service.GetAutocompleteSuggestions("st");

        Assert.Contains("status", commandSuggestions);
        Assert.Contains("stop", commandSuggestions);

        var argumentSuggestions = service.GetAutocompleteSuggestions("deploy f");

        Assert.Contains("deploy fast", argumentSuggestions);
    }

    [Fact]
    public void RegisterSameName_ReplacesAndWarns()
    {
        var service = new CommandSystemService(_ => { });

        service.RegisterCommand("x", _ => Task.CompletedTask, "first");
        service.RegisterCommand("x", _ => Task.CompletedTask, "second");

        var definition = Assert.Single(service.GetRegisteredCommands());
        Assert.Equal("second", definition.Description);
    }
}
