using SquidStd.AspNetCore.Extensions;
using SquidStd.Services.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

#region step-1

builder.UseSquidStd(
    o => o.ConfigName = "squidstd",
    container => container.RegisterCoreServices()
);

#endregion

#region step-2

builder.AddSquidStdHealthChecks();

#endregion

#region step-3

var app = builder.Build();
app.MapHealthChecks("/health");
app.MapGet("/", () => "SquidStd up");
app.Run();

#endregion
