using Testcontainers.Minio;

namespace SquidStd.Tests.Storage.S3;

/// <summary>
///     Starts a MinIO container once for the whole collection and exposes its endpoint and credentials.
/// </summary>
public sealed class MinioContainerFixture : IAsyncLifetime
{
    private readonly MinioContainer _container = new MinioBuilder().Build();

    public string Endpoint => $"{_container.Hostname}:{_container.GetMappedPublicPort(9000)}";

    public string ServiceUrl => $"http://{Endpoint}";

    public string AccessKey => _container.GetAccessKey();

    public string SecretKey => _container.GetSecretKey();

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
public sealed class MinioCollection : ICollectionFixture<MinioContainerFixture>
{
    public const string Name = "Minio";
}
