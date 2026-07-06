using System.Reflection;
using SquidStd.Core.Extensions.Env;

namespace SquidStd.Core.Config.Internal;

/// <summary>
/// Recursively substitutes environment-variable tokens into string properties of a bound
/// configuration instance and its nested SquidStd-namespaced class properties.
/// </summary>
internal static class ConfigEnvSubstitution
{
    /// <summary>
    /// Applies environment-variable substitution to <paramref name="instance" /> and its nested
    /// properties.
    /// </summary>
    /// <param name="instance">The configuration instance to process.</param>
    public static void Apply(object? instance)
        => Apply(instance, new(ReferenceEqualityComparer.Instance));

    private static void Apply(object? instance, HashSet<object> visited)
    {
        if (instance is null || !visited.Add(instance))
        {
            return;
        }

        var type = instance.GetType();

        if (type.Namespace is null || !type.Namespace.StartsWith("SquidStd", StringComparison.Ordinal))
        {
            return;
        }

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            if (property.PropertyType == typeof(string) && property.CanRead && property.CanWrite)
            {
                var current = (string?)property.GetValue(instance);

                if (!string.IsNullOrEmpty(current))
                {
                    property.SetValue(instance, current.ReplaceEnv());
                }

                continue;
            }

            if (property.PropertyType.IsClass && property.PropertyType != typeof(string) && property.CanRead)
            {
                Apply(property.GetValue(instance), visited);
            }
        }
    }
}
