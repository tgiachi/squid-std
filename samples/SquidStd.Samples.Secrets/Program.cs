using System.Text;
using SquidStd.Core.Interfaces.Secrets;
using SquidStd.Secrets.Aws.Extensions;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Services.Core.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;

var bootstrap = SquidStdBootstrap.Create(
    new SquidStdOptions()
    {
        ConfigName = "squidstd",
        RootDirectory = AppContext.BaseDirectory
    }
);

#region step-1

// Wire the KMS-backed protector and the Secrets Manager store against a LocalStack endpoint.
bootstrap.ConfigureServices(
    container =>
    {
        // Core services are registered one by one here: RegisterCoreServices() would also add
        // the default AES-GCM protector and file secret store, which must not compete with the
        // KMS protector and AWS Secrets Manager store registered below.
        container.RegisterDataSerializer();
        container.RegisterEventBusService();
        container.RegisterJobSystemService();
        container.RegisterMainThreadDispatcherService();
        container.RegisterTimerWheelService();
        container.RegisterMetricsCollectionService();

        container.RegisterKmsSecretProtector(
            options =>
            {
                options.KeyId = "alias/app";
                options.Aws = new()
                {
                    Region = "us-east-1",
                    ServiceUrl = "http://localhost:4566"
                };
            }
        );

        container.RegisterAwsSecretsManagerStore(
            options =>
            {
                options.NamePrefix = "myapp/";
                options.Aws = new()
                {
                    Region = "us-east-1",
                    ServiceUrl = "http://localhost:4566"
                };
            }
        );

        return container;
    }
);

#endregion

await bootstrap.StartAsync();

var protector = bootstrap.Resolve<ISecretProtector>();
var store = bootstrap.Resolve<ISecretStore>();

// The calls below need a live KMS + Secrets Manager endpoint (e.g. LocalStack), so they
// only run when explicitly enabled. The wiring above compiles and resolves without AWS.
if (Environment.GetEnvironmentVariable("SQUIDSTD_RUN_AWS") is null)
{
    Console.WriteLine("Set SQUIDSTD_RUN_AWS=1 with LocalStack running to exercise the live calls.");
    await bootstrap.StopAsync();

    return;
}

#region step-2

// Envelope-encrypt a value with a KMS data key, then decrypt it back.
var protectedBytes = protector.Protect(Encoding.UTF8.GetBytes("api-token"));
var plaintext = protector.Unprotect(protectedBytes);

Console.WriteLine($"Unprotected: {Encoding.UTF8.GetString(plaintext)}");

#endregion

#region step-3

// Store, fetch, and list secrets through the Secrets Manager store.
await store.SetAsync("db-password", "s3cr3t");
var password = await store.GetAsync("db-password");
Console.WriteLine($"db-password: {password}");

await foreach (var name in store.ListNamesAsync())
{
    Console.WriteLine($"secret: {name}");
}

#endregion

await bootstrap.StopAsync();
