namespace SquidStd.Core.Data.Events;

/// <summary>
///     Options controlling event bus dispatch behavior.
/// </summary>
public sealed record EventBusOptions
{
    /// <summary>
    ///     Listeners whose handling exceeds this duration are logged as slow. Defaults to 100ms.
    /// </summary>
    public TimeSpan SlowListenerThreshold { get; init; } = TimeSpan.FromMilliseconds(100);
}
