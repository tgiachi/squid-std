using SquidStd.AspNetCore.Extensions;
using SquidStd.Services.Core.Extensions;
using SquidStd.Workers.Manager.Extensions;
#if (messaging == "rabbitmq")
using SquidStd.Messaging.RabbitMq.Extensions;
#else
using SquidStd.Messaging.Extensions;
#endif

var builder = WebApplication.CreateBuilder(args);

builder.UseSquidStd(
    options => options.ConfigName = "squidstd",
    container =>
    {
        container.RegisterCoreServices();

#if (messaging == "rabbitmq")
        container.AddRabbitMqMessaging("rabbitmq://guest:guest@localhost:5672");
#else
        container.AddInMemoryMessaging();
#endif
        container.AddWorkerManager();

        return container;
    });

var app = builder.Build();

app.MapWorkerManagerEndpoints();

app.Run();
