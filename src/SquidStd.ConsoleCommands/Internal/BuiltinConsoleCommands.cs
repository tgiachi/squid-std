using SquidStd.ConsoleCommands.Interfaces;
using SquidStd.Core.Interfaces.Bootstrap;

namespace SquidStd.ConsoleCommands.Internal;

/// <summary>
/// Registers the built-in console commands (help, clear, exit).
/// </summary>
internal static class BuiltinConsoleCommands
{
    internal static void Register(ICommandSystemService system, ISquidStdBootstrap? bootstrap, Action clearScreen)
    {
        system.RegisterCommand(
            "help|?",
            context =>
            {
                foreach (var definition in system.GetRegisteredCommands())
                {
                    var aliases = definition.Aliases.Count > 0
                                      ? $" ({string.Join(", ", definition.Aliases)})"
                                      : string.Empty;
                    context.WriteLine($"{definition.Name}{aliases} - {definition.Description}");
                }

                return Task.CompletedTask;
            },
            "Lists the registered commands."
        );

        system.RegisterCommand(
            "clear|cls",
            _ =>
            {
                clearScreen();

                return Task.CompletedTask;
            },
            "Clears the console."
        );

        system.RegisterCommand(
            "exit|quit",
            context =>
            {
                if (bootstrap is null)
                {
                    context.WriteLine("Shutdown is not available: no bootstrap registered.");

                    return Task.CompletedTask;
                }

                context.WriteLine("Shutting down...");
                _ = Task.Run(() => bootstrap.StopAsync().AsTask());

                return Task.CompletedTask;
            },
            "Stops the host."
        );
    }
}
