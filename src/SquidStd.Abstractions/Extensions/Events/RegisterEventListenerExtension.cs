using DryIoc;
using SquidStd.Abstractions.Data.Internal.Events;
using SquidStd.Abstractions.Extensions.Container;
using SquidStd.Core.Interfaces.Events;

namespace SquidStd.Abstractions.Extensions.Events;

/// <summary>
///     Registers event listeners for DI-native auto-subscription at bootstrap.
/// </summary>
public static class RegisterEventListenerExtension
{
    /// <summary>
    ///     Registers a listener implementation as a singleton and records it for auto-subscription.
    /// </summary>
    /// <typeparam name="TEvent">The event type the listener handles.</typeparam>
    /// <typeparam name="TListener">The listener implementation type.</typeparam>
    /// <param name="container">The DryIoc container.</param>
    /// <returns>The same container for chaining.</returns>
    public static IContainer RegisterEventListener<TEvent, TListener>(this IContainer container)
        where TEvent : IEvent
        where TListener : class, IEventListener<TEvent>
    {
        container.Register<TListener>(Reuse.Singleton);
        container.AddToRegisterTypedList(
            new EventListenerRegistration(
                typeof(TListener),
                (bus, resolver) => bus.RegisterListener(resolver.Resolve<TListener>())
            )
        );

        return container;
    }
}
