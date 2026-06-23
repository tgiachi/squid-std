using Serilog;
using SquidStd.Workers.Abstractions.Data;
using SquidStd.Workers.Interfaces;

namespace SquidStd.Worker.Template;

/// <summary>
/// Sample job handler. Replace with your own <see cref="IJobHandler" /> implementations.
/// </summary>
public sealed class GreetJobHandler : IJobHandler
{
    private readonly ILogger _logger = Log.ForContext<GreetJobHandler>();

    public string JobName => "greet";

    public Task HandleAsync(JobRequest job, CancellationToken cancellationToken)
    {
        var name = job.Parameters.TryGetValue("name", out var value) ? value : "world";
        _logger.Information("Hello, {Name}!", name);

        return Task.CompletedTask;
    }
}
