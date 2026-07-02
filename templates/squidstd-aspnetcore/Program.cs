using SquidStd.AspNetCore.Extensions;
using SquidStd.Services.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.UseSquidStd(
    options =>
    {
        options.ConfigName = "squidstd";
    },
    container => container.RegisterCoreServices());
builder.AddSquidStdHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapGet("/hello", () => "Hello from SquidStd!");

app.Run();
