using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SquidStd.Generators.Common;
using SquidStd.Generators.Diagnostics;

namespace SquidStd.Generators.Workers;

[Generator(LanguageNames.CSharp)]
public sealed class JobHandlerRegistrationGenerator : IIncrementalGenerator
{
    private const string AttributeName = "SquidStd.Workers.Attributes.RegisterJobHandlerAttribute";
    private const string GeneratedFileName = "SquidStd.GeneratedJobHandlerRegistration.g.cs";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var candidates = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeName,
            static (node, _) => node is ClassDeclarationSyntax,
            static (context, cancellationToken) => CreateCandidate(context, cancellationToken)
        );

        context.RegisterSourceOutput(candidates.Collect(), static (context, candidates) => Execute(context, candidates));
    }

    private static JobHandlerRegistrationCandidate CreateCandidate(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var handlerType = (INamedTypeSymbol)context.TargetSymbol;
        var isSupported = GeneratorSymbolHelpers.IsConcreteNonGenericClass(handlerType)
                          && GeneratorSymbolHelpers.IsAccessibleFromGeneratedSource(handlerType)
                          && GeneratorSymbolHelpers.ImplementsInterface(
                              handlerType,
                              "IJobHandler",
                              "SquidStd.Workers.Interfaces"
                          );

        return new JobHandlerRegistrationCandidate(
            GeneratorSymbolHelpers.FullyQualified(handlerType),
            GeneratorSymbolHelpers.DisplayName(handlerType),
            GeneratorSymbolHelpers.PrimaryLocation(handlerType),
            isSupported
        );
    }

    private static void Execute(
        SourceProductionContext context,
        ImmutableArray<JobHandlerRegistrationCandidate> candidates
    )
    {
        var supported = new List<JobHandlerRegistrationCandidate>();
        var seenHandlerTypes = new HashSet<string>(StringComparer.Ordinal);

        foreach (var candidate in candidates)
        {
            if (!candidate.IsSupported)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        SquidStdGeneratorDiagnostics.UnsupportedJobHandler,
                        candidate.Location,
                        candidate.DisplayName
                    )
                );

                continue;
            }

            if (seenHandlerTypes.Add(candidate.HandlerTypeName))
            {
                supported.Add(candidate);
            }
        }

        supported.Sort(static (left, right) => string.Compare(
                left.HandlerTypeName,
                right.HandlerTypeName,
                StringComparison.Ordinal
            )
        );

        context.AddSource(GeneratedFileName, JobHandlerSourceBuilder.Build(supported));
    }
}
