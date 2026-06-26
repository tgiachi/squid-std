namespace SquidStd.Core.Interfaces.Events;

/// <summary>
/// In-process event bus that dispatches events to registered listeners in parallel.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an event and blocks until every listener has completed.
    /// </summary>
    /// <param name="eventData">The event payload.</param>
    /// <typeparam name="TEvent">The event type.</typeparam>
    void Publish<TEvent>(TEvent eventData) where TEvent : IEvent;

    /// <summary>
    /// Publishes an event and completes when every listener has finished.
    /// </summary>
    /// <param name="eventData">The event payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <returns>A task that completes after all listeners finish.</returns>
    Task PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default) where TEvent : IEvent;

    /// <summary>
    /// Registers a listener for the specified event type. Dispose the returned token to unsubscribe.
    /// </summary>
    /// <param name="listener">The listener to register.</param>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <returns>A token that unsubscribes the listener when disposed.</returns>
    IDisposable RegisterListener<TEvent>(IEventListener<TEvent> listener) where TEvent : IEvent;

    /// <summary>
    /// Subscribes a delegate handler for the specified event type. Dispose the returned token to unsubscribe.
    /// </summary>
    /// <param name="handler">The handler invoked for each published event.</param>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <returns>A token that unsubscribes the handler when disposed.</returns>
    IDisposable Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler) where TEvent : IEvent;
}
