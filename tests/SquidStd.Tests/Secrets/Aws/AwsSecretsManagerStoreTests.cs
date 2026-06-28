using SquidStd.Secrets.Aws.Data;
using SquidStd.Secrets.Aws.Services;
using SquidStd.Tests.Secrets.Aws.Support;

namespace SquidStd.Tests.Secrets.Aws;

[Collection(LocalStackSecretsCollection.Name)]
public class AwsSecretsManagerStoreTests
{
    private readonly LocalStackSecretsFixture _localStack;

    public AwsSecretsManagerStoreTests(LocalStackSecretsFixture localStack)
    {
        _localStack = localStack;
    }

    [Fact]
    public async Task Set_Get_Exists_List_Delete_RoundTrips()
    {
        var prefix = "test-" + Guid.NewGuid().ToString("N") + "/";
        var store = new AwsSecretsManagerStore(new AwsSecretsManagerOptions { Aws = _localStack.Aws, NamePrefix = prefix });

        Assert.Null(await store.GetAsync("db/main"));
        Assert.False(await store.ExistsAsync("db/main"));

        await store.SetAsync("db/main", "secret-1");
        await store.SetAsync("db/main", "secret-2"); // update existing

        Assert.Equal("secret-2", await store.GetAsync("db/main"));
        Assert.True(await store.ExistsAsync("db/main"));

        var names = new List<string>();
        await foreach (var name in store.ListNamesAsync())
        {
            names.Add(name);
        }

        Assert.Contains("db/main", names);

        Assert.True(await store.DeleteAsync("db/main"));
        Assert.Null(await store.GetAsync("db/main"));
        Assert.False(await store.DeleteAsync("db/main"));
    }
}
