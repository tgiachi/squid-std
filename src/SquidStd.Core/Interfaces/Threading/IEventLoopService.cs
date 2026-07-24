namespace SquidStd.Core.Interfaces.Threading;

/// <summary>
/// Exposes runtime metrics of the event loop. The loop lifecycle is provided by the implementation
/// (which also implements the service lifecycle interface); this interface stays free of an
/// Abstractions dependency because <c>SquidStd.Core</c> does not reference it.
/// </summary>
public interface IEventLoopService
{
    /// <summary>Number of loop iterations performed since start.</summary>
    long TickCount { get; }

    /// <summary>Exponential moving average of per-tick elapsed time in milliseconds.</summary>
    double AverageTickMs { get; }

    /// <summary>Worst observed tick elapsed time in milliseconds.</summary>
    double MaxTickMs { get; }

    /// <summary>True when the calling thread is the dedicated event-loop thread.</summary>
    bool IsOnLoopThread { get; }
}
