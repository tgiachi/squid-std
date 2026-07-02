using DryIoc;
using SquidStd.Services.Core.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;
using SquidStd.Workers.Extensions;
#if (messaging == "rabbitmq")
using SquidStd.Messaging.RabbitMq.Extensions;
#else
using SquidStd.Messaging.Extensions;
#endif

var bootstrap = SquidStdBootstrap.Create(options =>
{
    options.ConfigName = "squidstd";
});

bootstrap.ConfigureServices(container =>
{
    container.RegisterCoreServices();

#if (messaging == "rabbitmq")
    container.AddRabbitMqMessaging("rabbitmq://guest:guest@localhost:5672");
#else
    container.AddInMemoryMessaging();
#endif
    container.AddWorkers();
    container.AddJobHandler<GreetJobHandler>();

    return container;
});

await bootstrap.RunAsync();
