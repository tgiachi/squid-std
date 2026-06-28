using DryIoc;
using SquidStd.Core.Interfaces.Secrets;
using SquidStd.Secrets.Aws.Extensions;
using SquidStd.Secrets.Aws.Services;

namespace SquidStd.Tests.Secrets.Aws;

public class RegisterSecretsAwsExtensionsTests
{
    [Fact]
    public void RegisterKmsSecretProtector_RegistersSingletonProtector()
    {
        using var container = new Container();
        container.RegisterKmsSecretProtector(o => o.KeyId = "alias/test");

        var protector = container.Resolve<ISecretProtector>();
        Assert.IsType<KmsSecretProtector>(protector);
        Assert.Same(protector, container.Resolve<ISecretProtector>());
    }

    [Fact]
    public void RegisterAwsSecretsManagerStore_RegistersSingletonStore()
    {
        using var container = new Container();
        container.RegisterAwsSecretsManagerStore(o => { });

        var store = container.Resolve<ISecretStore>();
        Assert.IsType<AwsSecretsManagerStore>(store);
        Assert.Same(store, container.Resolve<ISecretStore>());
    }
}
