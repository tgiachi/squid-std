using DryIoc;
using SquidStd.Core.Interfaces.Events;

namespace SquidStd.Abstractions.Data.Internal.Events;

/// <summary>
/// A declarative event-listener registration consumed by the bootstrap activator.
/// The <see cref="Subscribe" /> closure captures the concrete event and listener types at
/// registration time, so subscription needs no reflection.
/// </summary>
/// <param name="ListenerType">The concrete listener implementation type.</param>
/// <param name="Subscribe">Resolves the listener and subscribes it to the bus.</param>
public sealed record EventListenerRegistration(Type ListenerType, Action<IEventBus, IResolverContext> Subscribe);
