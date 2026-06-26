using SquidStd.Core.Interfaces.Events;

namespace SquidStd.Services.Core.Services.Internal;

/// <summary>
///     Adapts a delegate handler to <see cref="IEventListener{TEvent}" /> so the bus has a single dispatch path.
/// </summary>
/// <typeparam name="TEvent">The event type.</typeparam>
internal sealed class DelegateEventListener<TEvent> : IEventListener<TEvent>
    where TEvent : IEvent
{
    private readonly Func<TEvent, CancellationToken, Task> _handler;

    public DelegateEventListener(Func<TEvent, CancellationToken, Task> handler)
    {
        _handler = handler;
    }

    public Task HandleAsync(TEvent eventData, CancellationToken cancellationToken = default)
    {
        return _handler(eventData, cancellationToken);
    }
}
