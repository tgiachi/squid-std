using SquidStd.Core.Data.Bootstrap;
using SquidStd.Services.Core.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;

#region step-1

var bootstrap = SquidStdBootstrap.Create(
    new SquidStdOptions()
    {
        ConfigName = "squidstd",
        RootDirectory = AppContext.BaseDirectory
    }
);

bootstrap.ConfigureServices(container => container.RegisterCoreServices());

#endregion

#region step-2

await bootstrap.StartAsync();

#endregion

#region step-3

await bootstrap.RunAsync();

#endregion
