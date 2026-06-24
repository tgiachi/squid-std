using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace SquidStd.Tests.Integration.Mail;

/// <summary>Starts a GreenMail container exposing SMTP/IMAP/POP3 on plain ports (auth disabled).</summary>
public sealed class GreenMailContainerFixture : IAsyncLifetime
{
    private readonly IContainer _container = new ContainerBuilder()
        .WithImage("greenmail/standalone:2.1.0")
        .WithEnvironment("GREENMAIL_OPTS", "-Dgreenmail.setup.test.all -Dgreenmail.hostname=0.0.0.0 -Dgreenmail.auth.disabled -Dgreenmail.verbose")
        .WithPortBinding(3025, true)
        .WithPortBinding(3143, true)
        .WithPortBinding(3110, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(3143))
        .Build();

    public string Host => _container.Hostname;

    public int SmtpPort => _container.GetMappedPublicPort(3025);

    public int ImapPort => _container.GetMappedPublicPort(3143);

    public int Pop3Port => _container.GetMappedPublicPort(3110);

    public Task InitializeAsync()
        => _container.StartAsync();

    public Task DisposeAsync()
        => _container.DisposeAsync().AsTask();
}

[CollectionDefinition(GreenMailCollection.Name)]
public sealed class GreenMailCollection : ICollectionFixture<GreenMailContainerFixture>
{
    public const string Name = "GreenMail";
}
