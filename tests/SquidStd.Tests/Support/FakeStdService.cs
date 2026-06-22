using SquidStd.Abstractions.Interfaces.Services;

namespace SquidStd.Tests.Support;

/// <summary>
/// Minimal <see cref="ISquidStdService" /> implementation tracking start/stop calls.
/// </summary>
public class FakeStdService : ISquidStdService
{
    /// <summary>
    /// Gets a value indicating whether the service is currently started.
    /// </summary>
    public bool Started { get; private set; }

    public ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        Started = true;

        return ValueTask.CompletedTask;
    }

    public ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        Started = false;

        return ValueTask.CompletedTask;
    }
}
