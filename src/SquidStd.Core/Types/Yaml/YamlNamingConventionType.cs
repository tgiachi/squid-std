namespace SquidStd.Core.Types.Yaml;

/// <summary>
/// Naming convention applied to YAML property keys during serialization and deserialization.
/// Section names in configuration files are dictionary keys and are never affected.
/// </summary>
public enum YamlNamingConventionType
{
    /// <summary>Properties as declared (C# convention makes this PascalCase). The default.</summary>
    PascalCase,

    /// <summary>camelCase keys.</summary>
    CamelCase,

    /// <summary>snake_case keys.</summary>
    SnakeCase,

    /// <summary>kebab-case keys.</summary>
    KebabCase,

    /// <summary>lowercase keys.</summary>
    LowerCase
}
