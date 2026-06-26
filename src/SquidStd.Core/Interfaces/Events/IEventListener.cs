namespace SquidStd.Core.Interfaces.Events;

/// <summary>
///     Handles an event published on the SquidStd event bus.
/// </summary>
/// <typeparam name="TEvent">The event type handled by the listener.</typeparam>
public interface IEventListener<in TEvent> where TEvent : IEvent
{
    /// <summary>
    ///     Handles a published event.
    /// </summary>
    /// <param name="eventData">The event payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the listener finishes handling the event.</returns>
    Task HandleAsync(TEvent eventData, CancellationToken cancellationToken = default);
}
