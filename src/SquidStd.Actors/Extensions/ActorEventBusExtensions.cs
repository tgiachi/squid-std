using SquidStd.Core.Interfaces.Events;

namespace SquidStd.Actors.Extensions;

/// <summary>
/// Bridges the event bus to an actor mailbox: published events are mapped to messages and delivered
/// in order through the actor's single consumer.
/// </summary>
public static class ActorEventBusExtensions
{
    /// <param name="actor">The actor whose mailbox receives the mapped messages.</param>
    extension<TMessage>(Actor<TMessage> actor)
    {
        /// <summary>
        /// Subscribes the actor to <typeparamref name="TEvent" /> on the bus, mapping each event to a
        /// message and telling it to the mailbox. Dispose the returned token to stop delivery.
        /// </summary>
        /// <param name="eventBus">The event bus to subscribe to.</param>
        /// <param name="map">Maps an event to an actor message.</param>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <returns>A token that unsubscribes when disposed.</returns>
        public IDisposable SubscribeToEventBus<TEvent>(IEventBus eventBus, Func<TEvent, TMessage> map)
            where TEvent : IEvent
            => eventBus.Subscribe<TEvent>(
                async (eventData, cancellationToken) =>
                {
                    await actor.TellAsync(map(eventData), cancellationToken);
                }
            );
    }
}
