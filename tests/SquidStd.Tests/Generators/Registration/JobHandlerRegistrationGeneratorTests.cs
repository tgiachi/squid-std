using SquidStd.Generators.Workers;
using SquidStd.Tests.Generators.Support;

namespace SquidStd.Tests.Generators.Registration;

public class JobHandlerRegistrationGeneratorTests
{
    [Fact]
    public void Run_GeneratesRegistrationExtension_WhenJobHandlerIsAnnotated()
    {
        const string source = """
            using System.Threading;
            using System.Threading.Tasks;
            using SquidStd.Workers.Abstractions.Data;
            using SquidStd.Workers.Attributes;
            using SquidStd.Workers.Interfaces;

            namespace SampleApp;

            [RegisterJobHandler]
            public sealed class GreetJobHandler : IJobHandler
            {
                public string JobName => "greet";

                public Task HandleAsync(JobRequest job, CancellationToken cancellationToken)
                {
                    return Task.CompletedTask;
                }
            }
            """;

        var result = GeneratorTestCompiler.Run(source, new JobHandlerRegistrationGenerator());
        var generatedSource = SingleGeneratedSource(result, "SquidStd.GeneratedJobHandlerRegistration.g.cs");

        Assert.Contains("RegisterGeneratedJobHandlers", generatedSource, StringComparison.Ordinal);
        Assert.Contains(
            "AddJobHandler<global::SampleApp.GreetJobHandler>(container);",
            generatedSource,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void Run_DoesNotRegisterJobHandler_WhenAttributeIsMissing()
    {
        const string source = """
            using System.Threading;
            using System.Threading.Tasks;
            using SquidStd.Workers.Abstractions.Data;
            using SquidStd.Workers.Interfaces;

            namespace SampleApp;

            public sealed class GreetJobHandler : IJobHandler
            {
                public string JobName => "greet";

                public Task HandleAsync(JobRequest job, CancellationToken cancellationToken)
                {
                    return Task.CompletedTask;
                }
            }
            """;

        var result = GeneratorTestCompiler.Run(source, new JobHandlerRegistrationGenerator());
        var generatedSource = SingleGeneratedSource(result, "SquidStd.GeneratedJobHandlerRegistration.g.cs");

        Assert.Contains("RegisterGeneratedJobHandlers", generatedSource, StringComparison.Ordinal);
        Assert.DoesNotContain("AddJobHandler<", generatedSource, StringComparison.Ordinal);
    }

    [Fact]
    public void Run_GeneratesNoOpExtension_WhenNoJobHandlersExist()
    {
        const string source = """
            namespace SampleApp;

            public sealed class EmptyType { }
            """;

        var result = GeneratorTestCompiler.Run(source, new JobHandlerRegistrationGenerator());
        var generatedSource = SingleGeneratedSource(result, "SquidStd.GeneratedJobHandlerRegistration.g.cs");

        Assert.Contains("RegisterGeneratedJobHandlers", generatedSource, StringComparison.Ordinal);
        Assert.Contains("return container;", generatedSource, StringComparison.Ordinal);
        Assert.DoesNotContain("AddJobHandler<", generatedSource, StringComparison.Ordinal);
    }

    [Fact]
    public void Run_ReportsDiagnostic_WhenAnnotatedTypeIsNotAJobHandler()
    {
        const string source = """
            using SquidStd.Workers.Attributes;

            namespace SampleApp;

            [RegisterJobHandler]
            public sealed class GreetJobHandler { }
            """;

        var result = GeneratorTestCompiler.Run(source, new JobHandlerRegistrationGenerator());
        var diagnostics = result.RunResult.Diagnostics.Concat(result.Diagnostics);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "SQDGEN004");
    }

    private static string SingleGeneratedSource(
        (Microsoft.CodeAnalysis.Compilation Compilation,
         Microsoft.CodeAnalysis.GeneratorDriverRunResult RunResult,
         System.Collections.Immutable.ImmutableArray<Microsoft.CodeAnalysis.Diagnostic> Diagnostics) result,
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
