using SquidStd.Core.Data.Bootstrap;
using SquidStd.Services.Core.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;
using SquidStd.Storage.Abstractions.Interfaces;
using SquidStd.Storage.Extensions;

var bootstrap = SquidStdBootstrap.Create(
    new SquidStdOptions()
    {
        ConfigName = "squidstd",
        RootDirectory = AppContext.BaseDirectory
    }
);

#region step-1

bootstrap.ConfigureServices(container => container.RegisterCoreServices().AddFileStorage());

#endregion

await bootstrap.StartAsync();

#region step-2

var storage = bootstrap.Resolve<IObjectStorageService>();

await storage.SaveAsync("user:1", new User { Name = "squid", Email = "squid@stormwind.it" });
var loaded = await storage.LoadAsync<User>("user:1");

Console.WriteLine($"{loaded?.Name} <{loaded?.Email}>");

#endregion

await bootstrap.StopAsync();

// YAML object storage needs a parameterless constructor, so the record uses init-only properties.
internal sealed record User
{
    public string Name { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;
}
