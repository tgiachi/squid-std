using SquidStd.Core.Data.Bootstrap;
using SquidStd.Core.Types.Yaml;
using SquidStd.Services.Core.Services.Bootstrap;
using SquidStd.Tests.Support;
using SquidStd.Abstractions.Extensions.Config;

namespace SquidStd.Tests.Bootstrap;

public class BootstrapYamlConventionTests
{
    public sealed class HostSection
    {
        public string ShardName { get; set; } = string.Empty;
    }

    [Fact]
    public void Create_WithSnakeCaseOption_BindsSnakeCaseFile()
    {
        using var root = new TempDirectory();
        File.WriteAllText(root.Combine("conv.yaml"), "host:\n  shard_name: Moongate\n");

        var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions
            {
                ConfigName = "conv",
                RootDirectory = root.Path,
                YamlNamingConvention = YamlNamingConventionType.SnakeCase
            }
        );

        bootstrap.ConfigureServices(c =>
        {
            c.RegisterConfigSection("host", static () => new HostSection());
            return c;
        });

        Assert.Equal("Moongate", bootstrap.Resolve<HostSection>().ShardName);
    }
}
