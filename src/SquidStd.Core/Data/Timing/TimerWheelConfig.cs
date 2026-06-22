namespace SquidStd.Core.Data.Timing;

/// <summary>
/// Configuration for the hashed timer wheel.
/// </summary>
public sealed class TimerWheelConfig
{
    /// <summary>
    /// Gets or sets the timer wheel granularity.
    /// </summary>
    public TimeSpan TickDuration { get; set; } = TimeSpan.FromMilliseconds(8);

    /// <summary>
    /// Gets or sets the number of slots in the wheel.
    /// </summary>
    public int WheelSize { get; set; } = 512;
}
