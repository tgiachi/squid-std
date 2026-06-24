using SquidStd.Core.Interfaces.Timing;

namespace SquidStd.Tests.Mail.Support;

/// <summary>Captures timer registration for tests without running a real timer wheel.</summary>
public sealed class FakeTimerService : ITimerService
{
    public string? RegisteredName { get; private set; }

    public string RegisterTimer(string name, TimeSpan interval, Action callback, TimeSpan? delay = null, bool repeat = false)
    {
        RegisteredName = name;

        return "timer-1";
    }

    public void UnregisterAllTimers()
    {
    }

    public bool UnregisterTimer(string timerId)
        => true;

    public int UnregisterTimersByName(string name)
        => 0;

    public int UpdateTicksDelta(long timestampMilliseconds)
        => 0;
}
