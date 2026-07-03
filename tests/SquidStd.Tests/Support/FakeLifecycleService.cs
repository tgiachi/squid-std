using SquidStd.Abstractions.Interfaces.Services;

namespace SquidStd.Tests.Support;

/// <summary>
/// Configurable <see cref="ISquidStdService" /> test double: can throw on start or stop and
/// records whether it was stopped. Not sealed: tests derive empty subclasses when they need
/// two distinct service types.
/// </summary>
public class FakeLifecycleService : ISquidStdService
{
    public bool ThrowOnStart { get; set; }

    public bool ThrowOnStop { get; set; }

    public bool Stopped { get; private set; }

    public ValueTask StartAsync(CancellationToken cancellationToken = default)
        => ThrowOnStart ? throw new InvalidOperationException("start boom") : ValueTask.CompletedTask;

    public ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        if (ThrowOnStop)
        {
            throw new InvalidOperationException("stop boom");
        }

        Stopped = true;

        return ValueTask.CompletedTask;
    }
}
