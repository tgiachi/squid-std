using SquidStd.Core.Data.Bootstrap;
using SquidStd.Core.Interfaces.Serialization;
using SquidStd.Services.Core.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;
using SquidStd.Tests.Support;

namespace SquidStd.Tests.Services.Core;

public class RegisterYamlDataSerializerTests
{
    [Fact]
    public void RegisterYamlDataSerializer_RegistersBothInterfaces()
    {
        using var root = new TempDirectory();
        var bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "yamlser", RootDirectory = root.Path }
        );

        bootstrap.ConfigureServices(c => c.RegisterYamlDataSerializer());

        Assert.Same(
            bootstrap.Resolve<IDataSerializer>(),
            bootstrap.Resolve<IDataDeserializer>()
        );
    }
}
