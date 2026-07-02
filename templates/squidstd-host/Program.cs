using SquidStd.Services.Core.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;

var bootstrap = SquidStdBootstrap.Create(options =>
{
    options.ConfigName = "squidstd";
});

bootstrap.ConfigureServices(container => container.RegisterCoreServices());

await bootstrap.RunAsync();
