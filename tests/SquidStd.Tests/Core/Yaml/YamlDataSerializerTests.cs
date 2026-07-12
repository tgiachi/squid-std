using System.Text;
using SquidStd.Core.Types.Yaml;
using SquidStd.Core.Yaml;
using YamlDotNet.Core;

namespace SquidStd.Tests.Core.Yaml;

public class YamlDataSerializerTests
{
    public sealed class SamplePayload
    {
        public string PlayerName { get; set; } = string.Empty;

        public int MaxLevel { get; set; }
    }

    [Theory]
    [InlineData(YamlNamingConventionType.PascalCase, "PlayerName")]
    [InlineData(YamlNamingConventionType.CamelCase, "playerName")]
    [InlineData(YamlNamingConventionType.SnakeCase, "player_name")]
    [InlineData(YamlNamingConventionType.KebabCase, "player-name")]
    [InlineData(YamlNamingConventionType.LowerCase, "playername")]
    public void Serialize_EmitsKeysInTheConfiguredConvention(YamlNamingConventionType convention, string expectedKey)
    {
        var serializer = new YamlDataSerializer(convention);

        var yaml = Encoding.UTF8.GetString(
            serializer.Serialize(new SamplePayload { PlayerName = "Hero", MaxLevel = 5 }).Span
        );

        Assert.Contains($"{expectedKey}:", yaml);
    }

    [Theory]
    [InlineData(YamlNamingConventionType.PascalCase)]
    [InlineData(YamlNamingConventionType.CamelCase)]
    [InlineData(YamlNamingConventionType.SnakeCase)]
    [InlineData(YamlNamingConventionType.KebabCase)]
    [InlineData(YamlNamingConventionType.LowerCase)]
    public void RoundTrip_PreservesValues(YamlNamingConventionType convention)
    {
        var serializer = new YamlDataSerializer(convention);
        var original = new SamplePayload { PlayerName = "Héroïne", MaxLevel = 42 };

        var restored = serializer.Deserialize<SamplePayload>(serializer.Serialize(original));

        Assert.Equal(original.PlayerName, restored.PlayerName);
        Assert.Equal(original.MaxLevel, restored.MaxLevel);
    }

    [Fact]
    public void Deserialize_UnknownKey_PermissiveIgnores_StrictThrows()
    {
        var yaml = "PlayerName: Hero\nGhostKey: boo\n"u8.ToArray();

        var permissive = new YamlDataSerializer();
        Assert.Equal("Hero", permissive.Deserialize<SamplePayload>(yaml).PlayerName);

        var strict = new YamlDataSerializer(ignoreUnmatchedProperties: false);
        Assert.Throws<YamlException>(() => strict.Deserialize<SamplePayload>(yaml));
    }

    [Fact]
    public void Deserialize_NullResult_Throws()
    {
        var serializer = new YamlDataSerializer();

        var ex = Assert.Throws<InvalidOperationException>(
            () => serializer.Deserialize<SamplePayload>(ReadOnlyMemory<byte>.Empty)
        );

        Assert.Contains(nameof(SamplePayload), ex.Message);
    }
}
