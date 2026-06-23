using SquidStd.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.UseSquidStd(options =>
{
    options.ConfigName = "squidstd";
});
builder.AddSquidStdHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapGet("/hello", () => "Hello from SquidStd!");

app.Run();
