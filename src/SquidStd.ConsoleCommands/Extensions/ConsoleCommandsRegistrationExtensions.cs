using DryIoc;
using Serilog.Core;
using SquidStd.Abstractions.Extensions.Config;
using SquidStd.ConsoleCommands.Data.Config;
using SquidStd.ConsoleCommands.Interfaces;
using SquidStd.ConsoleCommands.Internal;
using SquidStd.ConsoleCommands.Internal.Logging;
using SquidStd.ConsoleCommands.Services;
using SquidStd.Core.Interfaces.Bootstrap;

namespace SquidStd.ConsoleCommands.Extensions;

/// <summary>
/// Registration helpers for the interactive console command stack.
/// </summary>
public static class ConsoleCommandsRegistrationExtensions
{
    /// <param name="container">Container that receives the registrations.</param>
    extension(IContainer container)
    {
        /// <summary>
        /// Registers the console command stack: the prompt UI, the command system with the
        /// built-in help/clear/exit commands, and the Serilog sink that renders log lines above
        /// the prompt. Set <c>logger.EnableConsole: false</c> so the sink replaces the standard
        /// console sink.
        /// </summary>
        /// <returns>The same container for chaining.</returns>
        public IContainer AddConsoleCommands()
        {
            container.RegisterConfigSection("consoleCommands", static () => new ConsoleCommandsConfig(), -50);
            container.Register<IConsoleUiService, ConsoleUiService>(Reuse.Singleton);
            container.RegisterDelegate<ICommandSystemService>(
                resolver =>
                {
                    var ui = resolver.Resolve<IConsoleUiService>();
                    var system = new CommandSystemService(ui.WriteLine);
                    var bootstrap = resolver.Resolve<ISquidStdBootstrap>(IfUnresolved.ReturnDefault);
                    BuiltinConsoleCommands.Register(
                        system,
                        bootstrap,
                        static () =>
                        {
                            if (!Console.IsOutputRedirected)
                            {
                                Console.Clear();
                            }
                        }
                    );

                    return system;
                },
                Reuse.Singleton
            );
            container.RegisterDelegate<ILogEventSink>(
                resolver => new ConsolePromptLogSink(resolver.Resolve<IConsoleUiService>()),
                Reuse.Singleton
            );

            return container;
        }
    }
}
