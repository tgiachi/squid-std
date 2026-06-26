using Serilog.Core;
using Serilog.Events;

namespace SquidStd.Tests.Support;

/// <summary>
/// In-memory Serilog sink that records emitted log events for assertions.
/// </summary>
public sealed class CapturingLogSink : ILogEventSink
{
    private readonly List<LogEvent> _events = [];
    private readonly Lock _sync = new();

    public IReadOnlyList<LogEvent> Events
    {
        get
        {
            lock (_sync)
            {
                return [.. _events];
            }
        }
    }

    public void Emit(LogEvent logEvent)
    {
        lock (_sync)
        {
            _events.Add(logEvent);
        }
    }
}
