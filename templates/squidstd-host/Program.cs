using SquidStd.Services.Core.Services.Bootstrap;

var bootstrap = SquidStdBootstrap.Create(options =>
{
    options.ConfigName = "squidstd";
});

await bootstrap.RunAsync();
