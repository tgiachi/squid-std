using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SquidStd.Generators.Common;
using SquidStd.Generators.Diagnostics;

namespace SquidStd.Generators.ConsoleCommands;

[Generator(LanguageNames.CSharp)]
public sealed class ConsoleCommandRegistrationGenerator : IIncrementalGenerator
{
    private const string AttributeName = "SquidStd.ConsoleCommands.Attributes.RegisterConsoleCommandAttribute";
    private const string GeneratedFileName = "SquidStd.GeneratedConsoleCommandRegistration.g.cs";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var candidates = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeName,
            static (node, _) => node is ClassDeclarationSyntax,
            static (context, cancellationToken) => CreateCandidate(context, cancellationToken)
        );

        context.RegisterSourceOutput(candidates.Collect(), static (context, candidates) => Execute(context, candidates));
    }

    private static ConsoleCommandRegistrationCandidate CreateCandidate(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var executorType = (INamedTypeSymbol)context.TargetSymbol;
        var attribute = context.Attributes[0];
        var commandName = GetStringConstructorArgument(attribute, 0);
        var description = GetStringConstructorArgument(attribute, 1);

        var isSupported = !string.IsNullOrWhiteSpace(commandName) &&
                          GeneratorSymbolHelpers.IsConcreteNonGenericClass(executorType) &&
                          GeneratorSymbolHelpers.IsAccessibleFromGeneratedSource(executorType) &&
                          GeneratorSymbolHelpers.ImplementsInterface(
                              executorType,
                              "IConsoleCommandExecutor",
                              "SquidStd.ConsoleCommands.Interfaces"
                          );

        return new(
            GeneratorSymbolHelpers.FullyQualified(executorType),
            commandName ?? string.Empty,
            description ?? string.Empty,
            GeneratorSymbolHelpers.DisplayName(executorType),
            GeneratorSymbolHelpers.PrimaryLocation(executorType),
            isSupported
        );
    }

    private static string? GetStringConstructorArgument(AttributeData attribute, int index)
    {
        if (attribute.ConstructorArguments.Length <= index)
        {
            return null;
        }

        return attribute.ConstructorArguments[index].Value as string;
    }

    private static void Execute(
        SourceProductionContext context,
        ImmutableArray<ConsoleCommandRegistrationCandidate> candidates
    )
    {
        var supported = new List<ConsoleCommandRegistrationCandidate>();
        var seenExecutorTypes = new HashSet<string>(StringComparer.Ordinal);

        foreach (var candidate in candidates)
        {
            if (!candidate.IsSupported)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        SquidStdGeneratorDiagnostics.UnsupportedConsoleCommand,
                        candidate.Location,
                        candidate.DisplayName
                    )
                );

                continue;
            }

            if (seenExecutorTypes.Add(candidate.ExecutorTypeName))
            {
                supported.Add(candidate);
            }
        }

        supported.Sort(
            static (left, right) => string.Compare(
                left.ExecutorTypeName,
                right.ExecutorTypeName,
                StringComparison.Ordinal
            )
        );

        context.AddSource(GeneratedFileName, ConsoleCommandSourceBuilder.Build(supported));
    }
}
