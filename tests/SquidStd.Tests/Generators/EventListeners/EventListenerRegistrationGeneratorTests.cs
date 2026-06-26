using SquidStd.Tests.Generators.Support;

namespace SquidStd.Tests.Generators.EventListeners;

public class EventListenerRegistrationGeneratorTests
{
    [Fact]
    public void Run_GeneratesRegistrationExtension_WhenListenerExists()
    {
        const string source = """
            using System.Threading;
            using System.Threading.Tasks;
            using SquidStd.Core.Interfaces.Events;

            namespace SampleApp;

            public sealed record PingEvent(string Message) : IEvent;

            public sealed class PingListener : IEventListener<PingEvent>
            {
                public Task HandleAsync(PingEvent eventData, CancellationToken cancellationToken = default)
                {
                    return Task.CompletedTask;
                }
            }
            """;

        var result = GeneratorTestCompiler.Run(source);
        var generatedTree = Assert.Single(
            result.RunResult.GeneratedTrees,
            tree => tree.FilePath.EndsWith("SquidStd.GeneratedEventListenerRegistration.g.cs", StringComparison.Ordinal)
        );

        var generatedSource = generatedTree.GetText().ToString();

        Assert.Contains("RegisterGeneratedEventListeners", generatedSource, StringComparison.Ordinal);
        Assert.Contains(
            "RegisterEventListener<global::SampleApp.PingEvent, global::SampleApp.PingListener>(container);",
            generatedSource,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void Run_GeneratesNoOpExtension_WhenNoListenersExist()
    {
        const string source = """
            namespace SampleApp;

            public sealed class EmptyType
            {
            }
            """;

        var result = GeneratorTestCompiler.Run(source);
        var generatedTree = Assert.Single(
            result.RunResult.GeneratedTrees,
            tree => tree.FilePath.EndsWith("SquidStd.GeneratedEventListenerRegistration.g.cs", StringComparison.Ordinal)
        );

        var generatedSource = generatedTree.GetText().ToString();

        Assert.Contains("RegisterGeneratedEventListeners", generatedSource, StringComparison.Ordinal);
        Assert.Contains("return container;", generatedSource, StringComparison.Ordinal);
        Assert.DoesNotContain("RegisterEventListener<", generatedSource, StringComparison.Ordinal);
    }

    [Fact]
    public void Run_ReportsDiagnostic_WhenListenerCannotBeReferencedFromGeneratedSource()
    {
        const string source = """
            using System.Threading;
            using System.Threading.Tasks;
            using SquidStd.Core.Interfaces.Events;

            namespace SampleApp;

            public static class ListenerHost
            {
                private sealed record HiddenEvent(string Message) : IEvent;

                private sealed class HiddenListener : IEventListener<HiddenEvent>
                {
                    public Task HandleAsync(HiddenEvent eventData, CancellationToken cancellationToken = default)
                    {
                        return Task.CompletedTask;
                    }
                }
            }
            """;

        var result = GeneratorTestCompiler.Run(source);

        Assert.Contains(result.RunResult.Diagnostics, diagnostic => diagnostic.Id == "SQDGEN001");
    }
}
