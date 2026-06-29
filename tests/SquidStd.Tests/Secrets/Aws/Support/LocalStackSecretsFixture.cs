using SquidStd.Aws.Abstractions.Data.Config;
using Testcontainers.LocalStack;

namespace SquidStd.Tests.Secrets.Aws.Support;

/// <summary>Starts a LocalStack container (KMS + Secrets Manager) once for the collection.</summary>
public sealed class LocalStackSecretsFixture : IAsyncLifetime
{
    private readonly LocalStackContainer _container =
        new LocalStackBuilder().WithImage("localstack/localstack:3").Build();

    public AwsConfigEntry Aws => new()
    {
        Region = "us-east-1",
        AccessKey = "test",
        SecretKey = "test",
        ServiceUrl = _container.GetConnectionString()
    };

    public Task DisposeAsync()
    {
        return _container.DisposeAsync().AsTask();
    }

    public Task InitializeAsync()
    {
        return _container.StartAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class LocalStackSecretsCollection : ICollectionFixture<LocalStackSecretsFixture>
{
    public const string Name = "LocalStackSecrets";
}
