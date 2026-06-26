using System.Text.Json;
using System.Text.Json.Serialization;

namespace SquidStd.Tests.Support;

/// <summary>
///     No-op JSON converter used to exercise converter registration helpers.
/// </summary>
public class DummyGuidConverter : JsonConverter<Guid>
{
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Guid.Empty;
    }

    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
