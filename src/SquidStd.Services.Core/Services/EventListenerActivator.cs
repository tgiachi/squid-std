using DryIoc;
using SquidStd.Abstractions.Data.Internal.Events;
using SquidStd.Abstractions.Interfaces.Services;
using SquidStd.Core.Interfaces.Events;

namespace SquidStd.Services.Core.Services;

/// <summary>
/// Subscribes every DI-registered event listener to the bus at startup, before publishing services run.
/// </summary>
internal sealed class EventListenerActivator : ISquidStdService
{
    private readonly IContainer _container;
    private readonly IEventBus _eventBus;

    public EventListenerActivator(IContainer container, IEventBus eventBus)
    {
        _container = container;
        _eventBus = eventBus;
    }

    public ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        if (_container.IsRegistered<List<EventListenerRegistration>>())
        {
            var registrations = _container.Resolve<List<EventListenerRegistration>>();

            for (var i = 0; i < registrations.Count; i++)
            {
                registrations[i].Subscribe(_eventBus, _container);
            }
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask StopAsync(CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;
}
