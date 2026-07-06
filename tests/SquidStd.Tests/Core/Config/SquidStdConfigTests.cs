using SquidStd.Core.Config;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Core.Config;

public class SquidStdConfigTests
{
    public sealed class SampleSection
    {
        public string Name { get; set; } = "default";

        public int Value { get; set; } = 1;
    }

    public sealed class OtherSection
    {
        public bool Enabled { get; set; }
    }

    private static SquidStdConfig CreateWithYaml(TempDirectory root, string yaml)
    {
        File.WriteAllText(Path.Combine(root.Path, "app.yaml"), yaml);

        return SquidStdConfig.Load("app", root.Path);
    }

    [Fact]
    public void Load_MissingFile_YieldsDefaultsAndDoesNotCreateIt()
    {
        using var root = new TempDirectory();

        var config = SquidStdConfig.Load("app", root.Path);
        var section = config.GetSection<SampleSection>("sample");

        Assert.Equal("default", section.Name);
        Assert.False(File.Exists(config.ConfigPath));
    }

    [Fact]
    public void GetSection_BindsFromYaml_AndCachesSameInstance()
    {
        using var root = new TempDirectory();
        var config = CreateWithYaml(root, "sample:\n  Name: fromfile\n  Value: 42\n");

        var first = config.GetSection<SampleSection>("sample");
        var second = config.GetSection<SampleSection>("sample");

        Assert.Equal("fromfile", first.Name);
        Assert.Equal(42, first.Value);
        Assert.Same(first, second);
    }

    [Fact]
    public void GetSection_AppliesEnvSubstitution()
    {
        using var root = new TempDirectory();
        Environment.SetEnvironmentVariable("SQUIDSTD_TEST_NAME", "resolved");

        try
        {
            var config = CreateWithYaml(root, "sample:\n  Name: $SQUIDSTD_TEST_NAME\n");

            Assert.Equal("resolved", config.GetSection<SampleSection>("sample").Name);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SQUIDSTD_TEST_NAME", null);
        }
    }

    [Fact]
    public void GetSection_SameNameDifferentType_Throws()
    {
        using var root = new TempDirectory();
        var config = CreateWithYaml(root, "sample:\n  Name: x\n");

        config.GetSection<SampleSection>("sample");

        Assert.Throws<InvalidOperationException>(() => config.GetSection<OtherSection>("sample"));
    }

    [Fact]
    public void HasSection_ReflectsRawDocument()
    {
        using var root = new TempDirectory();
        var config = CreateWithYaml(root, "sample:\n  Name: x\n");

        Assert.True(config.HasSection("sample"));
        Assert.False(config.HasSection("ghost"));
    }

    [Fact]
    public void Reload_ClearsBindCache_AndRebindsFreshValues()
    {
        using var root = new TempDirectory();
        var config = CreateWithYaml(root, "sample:\n  Name: before\n");
        var before = config.GetSection<SampleSection>("sample");

        File.WriteAllText(config.ConfigPath, "sample:\n  Name: after\n");
        config.Reload();
        var after = config.GetSection<SampleSection>("sample");

        Assert.NotSame(before, after);
        Assert.Equal("after", after.Name);
    }

    [Fact]
    public void BindAll_BindsEveryTrackedSection()
    {
        using var root = new TempDirectory();
        var config = CreateWithYaml(root, "sample:\n  Value: 7\n");
        config.TrackSection("sample", typeof(SampleSection), 0, static () => new SampleSection());
        config.TrackSection("other", typeof(OtherSection), -10, static () => new OtherSection());

        var bound = config.BindAll();

        Assert.Equal(2, bound.Count);
        Assert.Equal(7, ((SampleSection)bound.Single(b => b.SectionName == "sample").Instance).Value);
    }

    [Fact]
    public void TrackSection_DuplicateSameNameAndType_IsIdempotent_DifferentTypeThrows()
    {
        using var root = new TempDirectory();
        var config = SquidStdConfig.Load("app", root.Path);

        config.TrackSection("sample", typeof(SampleSection), 0, static () => new SampleSection());
        config.TrackSection("sample", typeof(SampleSection), 0, static () => new SampleSection());

        Assert.Single(config.Entries);
        Assert.Throws<InvalidOperationException>(
            () => config.TrackSection("sample", typeof(OtherSection), 0, static () => new OtherSection())
        );
        Assert.Throws<InvalidOperationException>(
            () => config.TrackSection("sample2", typeof(SampleSection), 0, static () => new SampleSection())
        );
    }

    [Fact]
    public void ComposeAndSave_CoverBoundAndTrackedUnboundSections()
    {
        using var root = new TempDirectory();
        var config = CreateWithYaml(root, "sample:\n  Name: fromfile\n");
        config.GetSection<SampleSection>("sample");
        config.TrackSection("other", typeof(OtherSection), 10, static () => new OtherSection());

        var composed = config.Compose();

        Assert.Contains("fromfile", composed);
        Assert.Contains("other", composed);

        config.Save();
        Assert.Contains("other", File.ReadAllText(config.ConfigPath));
    }

    [Fact]
    public void Load_ResolvesTildeAndEnvInDirectory()
    {
        using var root = new TempDirectory();
        Environment.SetEnvironmentVariable("SQUIDSTD_TEST_DIR", root.Path);

        try
        {
            var config = SquidStdConfig.Load("app", "$SQUIDSTD_TEST_DIR");

            Assert.Equal(Path.GetFullPath(root.Path), config.ConfigDirectory);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SQUIDSTD_TEST_DIR", null);
        }
    }
}
