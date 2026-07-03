using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SquidStd.Generators.ConsoleCommands;
using SquidStd.Tests.Generators.Support;

namespace SquidStd.Tests.Generators.ConsoleCommands;

public class ConsoleCommandRegistrationGeneratorTests
{
    [Fact]
    public void Run_GeneratesRegistrationExtension_WhenConsoleCommandIsAnnotated()
    {
        const string source = """
                              using System.Threading.Tasks;
                              using SquidStd.ConsoleCommands.Attributes;
                              using SquidStd.ConsoleCommands.Data;
                              using SquidStd.ConsoleCommands.Interfaces;

                              namespace SampleApp;

                              [RegisterConsoleCommand("ping|p", "Replies pong.")]
                              public sealed class PingCommand : IConsoleCommandExecutor
                              {
                                  public string Description => "Replies pong.";

                                  public Task ExecuteAsync(ConsoleCommandContext context)
                                  {
                                      return Task.CompletedTask;
                                  }
                              }
                              """;

        var result = GeneratorTestCompiler.Run(source, new ConsoleCommandRegistrationGenerator());
        var generatedSource = SingleGeneratedSource(result, "SquidStd.GeneratedConsoleCommandRegistration.g.cs");

        Assert.Contains("RegisterGeneratedConsoleCommands", generatedSource, StringComparison.Ordinal);
        Assert.Contains("global::SampleApp.PingCommand", generatedSource, StringComparison.Ordinal);
        Assert.Contains(
            "RegisterCommand(\"ping|p\", executor.ExecuteAsync, \"Replies pong.\");",
            generatedSource,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void Run_WiresExecutorDescription_WhenAttributeDescriptionIsOmitted()
    {
        const string source = """
                              using System.Threading.Tasks;
                              using SquidStd.ConsoleCommands.Attributes;
                              using SquidStd.ConsoleCommands.Data;
                              using SquidStd.ConsoleCommands.Interfaces;

                              namespace SampleApp;

                              [RegisterConsoleCommand("ping")]
                              public sealed class PingCommand : IConsoleCommandExecutor
                              {
                                  public string Description => "Replies pong.";

                                  public Task ExecuteAsync(ConsoleCommandContext context)
                                  {
                                      return Task.CompletedTask;
                                  }
                              }
                              """;

        var result = GeneratorTestCompiler.Run(source, new ConsoleCommandRegistrationGenerator());
        var generatedSource = SingleGeneratedSource(result, "SquidStd.GeneratedConsoleCommandRegistration.g.cs");

        Assert.Contains(
            "RegisterCommand(\"ping\", executor.ExecuteAsync, executor.Description);",
            generatedSource,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void Run_DoesNotRegisterConsoleCommand_WhenAttributeIsMissing()
    {
        const string source = """
                              using System.Threading.Tasks;
                              using SquidStd.ConsoleCommands.Data;
                              using SquidStd.ConsoleCommands.Interfaces;

                              namespace SampleApp;

                              public sealed class PingCommand : IConsoleCommandExecutor
                              {
                                  public string Description => "Replies pong.";

                                  public Task ExecuteAsync(ConsoleCommandContext context)
                                  {
                                      return Task.CompletedTask;
                                  }
                              }
                              """;

        var result = GeneratorTestCompiler.Run(source, new ConsoleCommandRegistrationGenerator());
        var generatedSource = SingleGeneratedSource(result, "SquidStd.GeneratedConsoleCommandRegistration.g.cs");

        Assert.Contains("RegisterGeneratedConsoleCommands", generatedSource, StringComparison.Ordinal);
        Assert.DoesNotContain("RegisterCommand(", generatedSource, StringComparison.Ordinal);
    }

    [Fact]
    public void Run_GeneratesNoOpExtension_WhenNoConsoleCommandsExist()
    {
        const string source = """
                              namespace SampleApp;

                              public sealed class EmptyType { }
                              """;

        var result = GeneratorTestCompiler.Run(source, new ConsoleCommandRegistrationGenerator());
        var generatedSource = SingleGeneratedSource(result, "SquidStd.GeneratedConsoleCommandRegistration.g.cs");

        Assert.Contains("RegisterGeneratedConsoleCommands", generatedSource, StringComparison.Ordinal);
        Assert.Contains("return container;", generatedSource, StringComparison.Ordinal);
        Assert.DoesNotContain("RegisterCommand(", generatedSource, StringComparison.Ordinal);
    }

    [Fact]
    public void Run_ReportsDiagnostic_WhenAnnotatedTypeIsNotAConsoleCommand()
    {
        const string source = """
                              using SquidStd.ConsoleCommands.Attributes;

                              namespace SampleApp;

                              [RegisterConsoleCommand("ping|p", "Replies pong.")]
                              public sealed class PingCommand { }
                              """;

        var result = GeneratorTestCompiler.Run(source, new ConsoleCommandRegistrationGenerator());
        var diagnostics = result.RunResult.Diagnostics.Concat(result.Diagnostics);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "SQDGEN006");
    }

    [Fact]
    public void Run_ReportsDiagnostic_WhenCommandNameIsEmpty()
    {
        const string source = """
                              using System.Threading.Tasks;
                              using SquidStd.ConsoleCommands.Attributes;
                              using SquidStd.ConsoleCommands.Data;
                              using SquidStd.ConsoleCommands.Interfaces;

                              namespace SampleApp;

                              [RegisterConsoleCommand("")]
                              public sealed class PingCommand : IConsoleCommandExecutor
                              {
                                  public string Description => "Replies pong.";

                                  public Task ExecuteAsync(ConsoleCommandContext context)
                                  {
                                      return Task.CompletedTask;
                                  }
                              }
                              """;

        var result = GeneratorTestCompiler.Run(source, new ConsoleCommandRegistrationGenerator());
        var diagnostics = result.RunResult.Diagnostics.Concat(result.Diagnostics);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "SQDGEN006");
    }

    private static string SingleGeneratedSource(
        (Compilation Compilation,
            GeneratorDriverRunResult RunResult,
            ImmutableArray<Diagnostic> Diagnostics) result,
        string fileName
    )
    {
        var generatedTree = Assert.Single(
            result.RunResult.GeneratedTrees,
            tree => tree.FilePath.EndsWith(fileName, StringComparison.Ordinal)
        );

        return generatedTree.GetText().ToString();
    }
}
