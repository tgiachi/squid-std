using SquidStd.Core.Yaml;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Yaml;

public class YamlUtilsTests
{
    [Fact]
    public void SerializeDeserialize_RoundTrip_PreservesValues()
    {
        var original = new SampleDto { Name = "squid", Count = 42 };

        var yaml = YamlUtils.Serialize(original);
        var restored = YamlUtils.Deserialize<SampleDto>(yaml);

        Assert.Equal(original.Name, restored.Name);
        Assert.Equal(original.Count, restored.Count);
    }

    [Fact]
    public void Serialize_NullObject_Throws()
        => Assert.Throws<ArgumentNullException>(() => YamlUtils.Serialize<SampleDto>(null!));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Deserialize_NullOrWhitespace_Throws(string yaml)
        => Assert.Throws<ArgumentException>(() => YamlUtils.Deserialize<SampleDto>(yaml));

    [Fact]
    public void SerializeToFile_DeserializeFromFile_RoundTrips()
    {
        using var temp = new TempDirectory();
        var path = temp.Combine("nested/sample.yaml");

        YamlUtils.SerializeToFile(new SampleDto { Name = "file", Count = 9 }, path);
        var restored = YamlUtils.DeserializeFromFile<SampleDto>(path);

        Assert.True(File.Exists(path));
        Assert.Equal("file", restored.Name);
        Assert.Equal(9, restored.Count);
    }

    [Fact]
    public void DeserializeFromFile_MissingFile_Throws()
        => Assert.Throws<FileNotFoundException>(
            () => YamlUtils.DeserializeFromFile<SampleDto>(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".yaml"))
        );
}
