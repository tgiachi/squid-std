using SquidStd.Core.Types.Yaml;
using SquidStd.Core.Yaml;

namespace SquidStd.Tests.Core.Yaml;

public class YamlUtilsConventionTests
{
    public sealed class NetworkSection
    {
        public string BindAddress { get; set; } = string.Empty;

        public int MaxConnections { get; set; }
    }

    [Fact]
    public void DeserializeSection_SnakeCaseFile_BindsWithSnakeCaseConvention()
    {
        const string yaml = "network:\n  bind_address: 0.0.0.0\n  max_connections: 128\n";

        var section = (NetworkSection?)YamlUtils.DeserializeSection(
            yaml,
            "network",
            typeof(NetworkSection),
            YamlNamingConventionType.SnakeCase
        );

        Assert.Equal("0.0.0.0", section!.BindAddress);
        Assert.Equal(128, section.MaxConnections);
    }

    [Fact]
    public void SerializeSections_EmitsKeysInTheConfiguredConvention()
    {
        var sections = new Dictionary<string, object>
        {
            ["network"] = new NetworkSection { BindAddress = "0.0.0.0", MaxConnections = 128 }
        };

        var yaml = YamlUtils.SerializeSections(sections, YamlNamingConventionType.SnakeCase);

        Assert.Contains("network:", yaml);        // section name untouched
        Assert.Contains("bind_address:", yaml);   // property key converted
    }

    [Fact]
    public void Defaults_KeepPascalCaseBehavior()
    {
        const string yaml = "network:\n  BindAddress: 1.2.3.4\n";

        var section = (NetworkSection?)YamlUtils.DeserializeSection(yaml, "network", typeof(NetworkSection));

        Assert.Equal("1.2.3.4", section!.BindAddress);
        Assert.Contains("BindAddress:", YamlUtils.Serialize(new NetworkSection { BindAddress = "x" }));
    }
}
