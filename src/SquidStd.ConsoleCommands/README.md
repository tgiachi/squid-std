<h1 align="center">SquidStd.ConsoleCommands</h1>

Interactive console commands for SquidStd hosts, rendered on a fixed prompt at the bottom of the terminal.
A command registry supports aliases (`"gc|collect"`), quoted arguments, TAB autocomplete cycling, command
history and an optional locked mode that ignores input until an unlock character is pressed. Log lines are
rendered above the prompt through a Serilog sink, so the input row never gets torn by log output. When the
console is not interactive (redirected output, CI), the input loop disables itself and commands remain
callable programmatically.

## Install

```bash
dotnet add package SquidStd.ConsoleCommands
```

## Usage

```csharp
using DryIoc;
using SquidStd.ConsoleCommands.Extensions;
using SquidStd.ConsoleCommands.Interfaces;

bootstrap.ConfigureServices(container =>
{
    container.RegisterCoreServices();
    container.AddConsoleCommands();               // prompt UI, command system, builtins, log sink
    container.RegisterGeneratedConsoleCommands(); // attribute-registered commands (source generated)

    return container;
});

await bootstrap.StartAsync();

// Delegate-based registration.
var commands = bootstrap.Resolve<ICommandSystemService>();
commands.RegisterCommand("echo", ctx =>
{
    ctx.WriteLine(string.Join(' ', ctx.Arguments));
    return Task.CompletedTask;
}, "Echoes the arguments back.");

// Programmatic execution (also works when the console is not interactive).
foreach (var line in await commands.ExecuteCommandWithOutputAsync("help"))
{
    Console.WriteLine(line);
}
```

Class-based commands are picked up by the `SquidStd.Generators` source generator: annotate an
`IConsoleCommandExecutor` and call `RegisterGeneratedConsoleCommands()` (namespace
`SquidStd.Generators.ConsoleCommands`). The class is registered as a singleton in the container and wired
to the command system when it is resolved.

```csharp
using SquidStd.ConsoleCommands.Attributes;
using SquidStd.ConsoleCommands.Data;
using SquidStd.ConsoleCommands.Interfaces;

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
```

The builtin commands `help` (`?`), `clear` (`cls`) and `exit` (`quit`) are registered by
`AddConsoleCommands()`.

## Configuration

The `consoleCommands` section of the SquidStd YAML config controls the prompt:

```yaml
consoleCommands:
  Prompt: "myapp> "
  StartLocked: true
  UnlockCharacter: "*"
```

Set `logger.EnableConsole: false` so the prompt log sink replaces the standard console sink; otherwise
every log line is written twice.

## Key types

| Type                             | Purpose                                                                            |
|----------------------------------|------------------------------------------------------------------------------------|
| `ICommandSystemService`          | Registry and dispatcher: register, execute, collect output, autocomplete.          |
| `ConsoleCommandContext`          | Handler context: raw text, parsed arguments and the output writer.                 |
| `IConsoleCommandExecutor`        | Contract for class-based commands registered via the attribute.                    |
| `RegisterConsoleCommandAttribute`| Marks an executor for source-generated registration (name/aliases + description).  |
| `ConsoleCommandsConfig`          | `consoleCommands` config section: prompt text, locked start, unlock character.     |

## Related

- Docs: [SquidStd.ConsoleCommands](https://tgiachi.github.io/squid-std/articles/consolecommands.html)
- Docs: [SquidStd.Services.Core](https://tgiachi.github.io/squid-std/articles/services-core.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
