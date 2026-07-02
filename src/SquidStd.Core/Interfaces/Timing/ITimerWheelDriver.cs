namespace SquidStd.Core.Interfaces.Timing;

/// <summary>
/// Marker for a service that advances the timer wheel (<see cref="ITimerService.UpdateTicksDelta" />).
/// At most one driver must be registered at a time, so that the wheel is not advanced twice per elapsed
/// time. Implemented by the timer-wheel pump and by the event loop, which are mutually exclusive.
/// </summary>
public interface ITimerWheelDriver
{
}
