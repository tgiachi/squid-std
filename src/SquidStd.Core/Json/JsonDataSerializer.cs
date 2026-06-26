using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using SquidStd.Core.Interfaces.Serialization;

namespace SquidStd.Core.Json;

/// <summary>
///     Default JSON data serializer based on <see cref="System.Text.Json" /> Web defaults
///     (reflection-based, supports arbitrary types). Implements both <see cref="IDataSerializer" />
///     and <see cref="IDataDeserializer" />.
/// </summary>
public sealed class JsonDataSerializer : IDataSerializer, IDataDeserializer
{
    private readonly JsonSerializerOptions _options;

    public JsonDataSerializer()
    {
        _options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    [RequiresUnreferencedCode("JSON deserialization may require types that cannot be statically analyzed.")]
    [RequiresDynamicCode("JSON deserialization may require runtime code generation.")]
    public T Deserialize<T>(ReadOnlyMemory<byte> data)
    {
        return JsonSerializer.Deserialize<T>(data.Span, _options) ??
               throw new InvalidOperationException($"Deserialization returned null for type {typeof(T).Name}.");
    }

    [RequiresUnreferencedCode("JSON serialization may require types that cannot be statically analyzed.")]
    [RequiresDynamicCode("JSON serialization may require runtime code generation.")]
    public ReadOnlyMemory<byte> Serialize<T>(T value)
    {
        return JsonSerializer.SerializeToUtf8Bytes(value, _options);
    }
}
