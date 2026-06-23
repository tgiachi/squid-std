using System.Text.RegularExpressions;
using SquidStd.Search.Abstractions.Attributes;

namespace SquidStd.Search.Abstractions.Search;

/// <summary>
/// Resolves the Elasticsearch index name for a type from its <see cref="SearchIndexAttribute" /> (or the
/// lowercased type name), expanding <c>${VAR}</c> / <c>${VAR:-default}</c> from environment variables. The
/// result is always lowercased (an Elasticsearch requirement).
/// </summary>
public static partial class SearchIndexNameResolver
{
    /// <summary>Resolves the index name for <paramref name="type" />.</summary>
    public static string Resolve(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var template = type.GetCustomAttributes(typeof(SearchIndexAttribute), inherit: false) is { Length: > 0 } attributes
            && attributes[0] is SearchIndexAttribute attribute
                ? attribute.Name
                : type.Name;

        return ExpandEnvironment(template).ToLowerInvariant();
    }

    private static string ExpandEnvironment(string template)
        => PlaceholderRegex().Replace(
            template,
            match =>
            {
                var name = match.Groups["name"].Value;
                var hasDefault = match.Groups["default"].Success;
                var value = Environment.GetEnvironmentVariable(name);

                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }

                if (hasDefault)
                {
                    return match.Groups["default"].Value;
                }

                throw new InvalidOperationException(
                    $"Environment variable '{name}' is not set for index template '{template}'.");
            });

    [GeneratedRegex(@"\$\{(?<name>[A-Za-z_][A-Za-z0-9_]*)(:-(?<default>[^}]*))?\}")]
    private static partial Regex PlaceholderRegex();
}
