using SquidStd.Core.Types.Yaml;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SquidStd.Core.Yaml;

/// <summary>
/// Resolves a <see cref="YamlNamingConventionType" /> to the corresponding YamlDotNet
/// <see cref="INamingConvention" /> instance.
/// </summary>
internal static class YamlNamingConventions
{
    /// <summary>
    /// Resolves the YamlDotNet naming convention for the given <see cref="YamlNamingConventionType" />.
    /// </summary>
    /// <param name="convention">The naming convention to resolve.</param>
    /// <returns>The matching YamlDotNet <see cref="INamingConvention" />.</returns>
    public static INamingConvention Resolve(YamlNamingConventionType convention)
        => convention switch
        {
            YamlNamingConventionType.CamelCase => CamelCaseNamingConvention.Instance,
            YamlNamingConventionType.SnakeCase => UnderscoredNamingConvention.Instance,
            YamlNamingConventionType.KebabCase => HyphenatedNamingConvention.Instance,
            YamlNamingConventionType.LowerCase => LowerCaseNamingConvention.Instance,
            _ => NullNamingConvention.Instance
        };
}
