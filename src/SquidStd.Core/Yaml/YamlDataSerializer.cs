using System.Text;
using SquidStd.Core.Interfaces.Serialization;
using SquidStd.Core.Types.Yaml;
using YamlDotNet.Serialization;

namespace SquidStd.Core.Yaml;

/// <summary>
/// YAML data serializer based on YamlDotNet. Implements both <see cref="IDataSerializer" />
/// and <see cref="IDataDeserializer" /> with a configurable property naming convention and an
/// optional strict mode that rejects unknown YAML keys.
/// </summary>
public sealed class YamlDataSerializer : IDataSerializer, IDataDeserializer
{
    private readonly IDeserializer _deserializer;
    private readonly ISerializer _serializer;

    /// <summary>
    /// Initializes the serializer.
    /// </summary>
    /// <param name="convention">Property naming convention (PascalCase-as-declared by default).</param>
    /// <param name="ignoreUnmatchedProperties">
    /// When true (default) unknown YAML keys are ignored; when false they fail deserialization.
    /// </param>
    public YamlDataSerializer(
        YamlNamingConventionType convention = YamlNamingConventionType.PascalCase,
        bool ignoreUnmatchedProperties = true
    )
    {
        var namingConvention = YamlNamingConventions.Resolve(convention);

        _serializer = new SerializerBuilder()
                      .DisableAliases()
                      .WithIndentedSequences()
                      .WithNamingConvention(namingConvention)
                      .Build();

        var deserializerBuilder = new DeserializerBuilder().WithNamingConvention(namingConvention);

        if (ignoreUnmatchedProperties)
        {
            deserializerBuilder = deserializerBuilder.IgnoreUnmatchedProperties();
        }

        _deserializer = deserializerBuilder.Build();
    }

    /// <inheritdoc />
    public T Deserialize<T>(ReadOnlyMemory<byte> data)
    {
        var yaml = Encoding.UTF8.GetString(data.Span);

        if (string.IsNullOrWhiteSpace(yaml))
        {
            throw new InvalidOperationException($"Cannot deserialize empty or whitespace-only YAML for type {typeof(T).Name}.");
        }

        return _deserializer.Deserialize<T>(yaml) ??
               throw new InvalidOperationException($"Deserialization returned null for type {typeof(T).Name}.");
    }

    /// <inheritdoc />
    public ReadOnlyMemory<byte> Serialize<T>(T value)
        => Encoding.UTF8.GetBytes(_serializer.Serialize(value));
}
