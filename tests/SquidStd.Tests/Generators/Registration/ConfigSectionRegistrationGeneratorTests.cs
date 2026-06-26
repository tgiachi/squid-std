using SquidStd.Generators.Config;
using SquidStd.Tests.Generators.Support;

namespace SquidStd.Tests.Generators.Registration;

public class ConfigSectionRegistrationGeneratorTests
{
    [Fact]
    public void Run_GeneratesRegistrationExtension_WhenConfigIsAnnotated()
    {
        const string source = """
            using SquidStd.Abstractions.Attributes;

            namespace SampleApp;

            [RegisterConfigSection("workers", Priority = -50)]
            public sealed class WorkersConfig { }
            """;

        var result = GeneratorTestCompiler.Run(source, new ConfigSectionRegistrationGenerator());
        var generatedSource = SingleGeneratedSource(result, "SquidStd.GeneratedConfigSectionRegistration.g.cs");

        Assert.Contains("RegisterGeneratedConfigSections", generatedSource, StringComparison.Ordinal);
        Assert.Contains(
            "RegisterConfigSection<global::SampleApp.WorkersConfig>(container, \"workers\", priority: -50);",
            generatedSource,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void Run_DoesNotRegisterConfig_WhenAttributeIsMissing()
    {
        const string source = """
            namespace SampleApp;

            public sealed class WorkersConfig { }
            """;

        var result = GeneratorTestCompiler.Run(source, new ConfigSectionRegistrationGenerator());
        var generatedSource = SingleGeneratedSource(result, "SquidStd.GeneratedConfigSectionRegistration.g.cs");

        Assert.Contains("RegisterGeneratedConfigSections", generatedSource, StringComparison.Ordinal);
        Assert.DoesNotContain("RegisterConfigSection<", generatedSource, StringComparison.Ordinal);
    }

    [Fact]
    public void Run_GeneratesNoOpExtension_WhenNoConfigsExist()
    {
        const string source = """
            namespace SampleApp;

            public sealed class EmptyType { }
            """;

        var result = GeneratorTestCompiler.Run(source, new ConfigSectionRegistrationGenerator());
        var generatedSource = SingleGeneratedSource(result, "SquidStd.GeneratedConfigSectionRegistration.g.cs");

        Assert.Contains("RegisterGeneratedConfigSections", generatedSource, StringComparison.Ordinal);
        Assert.Contains("return container;", generatedSource, StringComparison.Ordinal);
        Assert.DoesNotContain("RegisterConfigSection<", generatedSource, StringComparison.Ordinal);
    }

    [Fact]
    public void Run_ReportsDiagnostic_WhenSectionNameIsMissing()
    {
        const string source = """
            using SquidStd.Abstractions.Attributes;

            namespace SampleApp;

            [RegisterConfigSection]
            public sealed class WorkersConfig { }
            """;

        var result = GeneratorTestCompiler.Run(source, new ConfigSectionRegistrationGenerator());
        var diagnostics = result.RunResult.Diagnostics.Concat(result.Diagnostics);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "SQDGEN003");
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
