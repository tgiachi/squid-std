using DryIoc;
using SquidStd.Core.Data.Bootstrap;
using SquidStd.Core.Interfaces.Events;
using SquidStd.Core.Types;
using SquidStd.Services.Core.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;

var bootstrap = SquidStdBootstrap.Create(options =>
{
    options.ConfigName = "squidstd";
});

bootstrap.ConfigureServices(container => container.RegisterCoreServices());

// Tweak a loaded config section before services start (in-memory only).
bootstrap.OnConfigLoaded<SquidStdLoggerOptions>(o => o.MinimumLevel = LogLevelType.Information);

// Inspect the final configuration once every hook has been applied.
bootstrap.OnConfigReady(cfg => Console.WriteLine(cfg.Compose()));

// React to the engine lifecycle events.
var bus = bootstrap.Container.Resolve<IEventBus>();
bus.Subscribe<EngineStartedEvent>((e, _) =>
{
    Console.WriteLine($"{e.Application} ready with {e.ServiceCount} service(s)");
    return Task.CompletedTask;
});
bus.Subscribe<EngineStoppedEvent>((e, _) =>
{
    Console.WriteLine($"{e.Application} stopped");
    return Task.CompletedTask;
});

await bootstrap.RunAsync();
