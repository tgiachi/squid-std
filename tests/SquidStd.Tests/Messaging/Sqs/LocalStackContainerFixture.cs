using SquidStd.Aws.Abstractions.Data.Config;
using Testcontainers.LocalStack;

namespace SquidStd.Tests.Messaging.Sqs;

/// <summary>
/// Starts a LocalStack container (SQS + SNS) once for the whole collection and exposes an
/// <see cref="AwsConfigEntry" /> pointing at its edge endpoint with dummy credentials.
/// </summary>
public sealed class LocalStackContainerFixture : IAsyncLifetime
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
        => _container.DisposeAsync().AsTask();

    public Task InitializeAsync()
        => _container.StartAsync();
}

[CollectionDefinition(Name)]
public sealed class LocalStackCollection : ICollectionFixture<LocalStackContainerFixture>
{
    public const string Name = "LocalStack";
}
