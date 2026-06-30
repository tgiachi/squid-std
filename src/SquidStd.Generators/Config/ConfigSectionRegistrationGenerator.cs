using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SquidStd.Generators.Common;
using SquidStd.Generators.Diagnostics;

namespace SquidStd.Generators.Config;

[Generator(LanguageNames.CSharp)]
public sealed class ConfigSectionRegistrationGenerator : IIncrementalGenerator
{
    private const string AttributeName = "SquidStd.Abstractions.Attributes.RegisterConfigSectionAttribute";
    private const string GeneratedFileName = "SquidStd.GeneratedConfigSectionRegistration.g.cs";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var candidates = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeName,
            static (node, _) => node is ClassDeclarationSyntax,
            static (context, cancellationToken) => CreateCandidate(context, cancellationToken)
        );

        context.RegisterSourceOutput(candidates.Collect(), static (context, candidates) => Execute(context, candidates));
    }

    private static ConfigSectionRegistrationCandidate CreateCandidate(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var configType = (INamedTypeSymbol)context.TargetSymbol;
        var attribute = context.Attributes[0];
        var sectionName = GetSectionName(attribute);
        var priority = GetIntNamedArgument(attribute, "Priority");

        var isSupported = !string.IsNullOrWhiteSpace(sectionName) &&
                          GeneratorSymbolHelpers.IsConcreteNonGenericClass(configType) &&
                          GeneratorSymbolHelpers.IsAccessibleFromGeneratedSource(configType) &&
                          GeneratorSymbolHelpers.HasPublicParameterlessConstructor(configType);

        return new(
            GeneratorSymbolHelpers.FullyQualified(configType),
            sectionName ?? string.Empty,
            GeneratorSymbolHelpers.DisplayName(configType),
            GeneratorSymbolHelpers.PrimaryLocation(configType),
            priority,
            isSupported
        );
    }

    private static string? GetSectionName(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length == 0)
        {
            return null;
        }

        return attribute.ConstructorArguments[0].Value as string;
    }

    private static int GetIntNamedArgument(AttributeData attribute, string name)
    {
        foreach (var argument in attribute.NamedArguments)
        {
            if (argument.Key == name && argument.Value.Value is int value)
            {
                return value;
            }
        }

        return 0;
    }

    private static void Execute(
        SourceProductionContext context,
        ImmutableArray<ConfigSectionRegistrationCandidate> candidates
    )
    {
        var supported = new List<ConfigSectionRegistrationCandidate>();
        var seenKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var candidate in candidates)
        {
            if (!candidate.IsSupported)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        SquidStdGeneratorDiagnostics.UnsupportedConfigSection,
                        candidate.Location,
                        candidate.DisplayName
                    )
                );

                continue;
            }

            var key = candidate.SectionName + "|" + candidate.ConfigTypeName;

            if (seenKeys.Add(key))
            {
                supported.Add(candidate);
            }
        }

        supported.Sort(
            static (left, right) =>
            {
                var sectionComparison = string.Compare(left.SectionName, right.SectionName, StringComparison.Ordinal);

                return sectionComparison != 0
                           ? sectionComparison
                           : string.Compare(left.ConfigTypeName, right.ConfigTypeName, StringComparison.Ordinal);
            }
        );

        context.AddSource(GeneratedFileName, ConfigSectionSourceBuilder.Build(supported));
    }
}
