namespace SquidStd.Core.Interfaces.Timing;

/// <summary>
///     Schedules timer callbacks on a hashed timer wheel.
/// </summary>
public interface ITimerService
{
    /// <summary>
    ///     Registers a timer.
    /// </summary>
    /// <param name="name">Logical timer name.</param>
    /// <param name="interval">Timer interval.</param>
    /// <param name="callback">Callback invoked when the timer fires.</param>
    /// <param name="delay">Initial delay before the first execution.</param>
    /// <param name="repeat">Whether the timer repeats after firing.</param>
    /// <returns>The registered timer id.</returns>
    string RegisterTimer(
        string name,
        TimeSpan interval,
        Action callback,
        TimeSpan? delay = null,
        bool repeat = false
    );

    /// <summary>
    ///     Removes all registered timers.
    /// </summary>
    void UnregisterAllTimers();

    /// <summary>
    ///     Removes a timer by id.
    /// </summary>
    /// <param name="timerId">Timer id to remove.</param>
    /// <returns><c>true</c> when a timer was removed; otherwise <c>false</c>.</returns>
    bool UnregisterTimer(string timerId);

    /// <summary>
    ///     Removes all timers with the specified name.
    /// </summary>
    /// <param name="name">Timer name to remove.</param>
    /// <returns>The number of removed timers.</returns>
    int UnregisterTimersByName(string name);

    /// <summary>
    ///     Advances the wheel using an absolute timestamp in milliseconds.
    /// </summary>
    /// <param name="timestampMilliseconds">Absolute monotonic timestamp in milliseconds.</param>
    /// <returns>The number of processed wheel ticks.</returns>
    int UpdateTicksDelta(long timestampMilliseconds);
}
