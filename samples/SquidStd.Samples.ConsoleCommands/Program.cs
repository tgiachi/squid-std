using SquidStd.ConsoleCommands.Attributes;
using SquidStd.ConsoleCommands.Data;
using SquidStd.ConsoleCommands.Extensions;
using SquidStd.ConsoleCommands.Interfaces;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Generators.ConsoleCommands;
using SquidStd.Services.Core.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;

var bootstrap = SquidStdBootstrap.Create(
    new SquidStdOptions
    {
        ConfigName = "squidstd",
        RootDirectory = AppContext.BaseDirectory,
        AppName = "ConsoleDemo"
    }
);

bootstrap.ConfigureServices(
    container =>
    {
        container.RegisterCoreServices();
        container.AddConsoleCommands();
        container.RegisterGeneratedConsoleCommands();

        return container;
    }
);

await bootstrap.StartAsync();

var commands = bootstrap.Resolve<ICommandSystemService>();
commands.RegisterCommand(
    "echo",
    ctx =>
    {
        ctx.WriteLine(string.Join(' ', ctx.Arguments));

        return Task.CompletedTask;
    },
    "Echoes the arguments back."
);

foreach (var line in await commands.ExecuteCommandWithOutputAsync("help"))
{
    Console.WriteLine(line);
}

foreach (var line in await commands.ExecuteCommandWithOutputAsync("ping"))
{
    Console.WriteLine(line);
}

await bootstrap.StopAsync();

[RegisterConsoleCommand("ping|p", "Replies pong.")]
internal sealed class PingCommand : IConsoleCommandExecutor
{
    public string Description => "Replies pong.";

    public Task ExecuteAsync(ConsoleCommandContext context)
    {
        context.WriteLine("pong");

        return Task.CompletedTask;
    }
}
