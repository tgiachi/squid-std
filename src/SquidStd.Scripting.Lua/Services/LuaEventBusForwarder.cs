using System.Collections.Concurrent;
using System.Reflection;
using Serilog;
using SquidStd.Abstractions.Interfaces.Services;
using SquidStd.Core.Extensions.Strings;
using SquidStd.Core.Interfaces.Events;
using SquidStd.Scripting.Lua.Interfaces.Events;

namespace SquidStd.Scripting.Lua.Services;

/// <summary>
/// Forwards every event published on the event bus to the Lua event bridge, using the
/// snake_case convention: the CLR type name minus the <c>Event</c> suffix
/// (<c>EngineStartedEvent</c> becomes <c>engine_started</c>) with snake_case payload keys.
/// No-op when no event bus is registered.
/// </summary>
public sealed class LuaEventBusForwarder : ISquidStdService, IDisposable
{
    private static readonly ConcurrentDictionary<Type, string> NameCache = new();
    private static readonly ConcurrentDictionary<Type, (string Key, Func<object, object?> Get)[]> PayloadCache = new();

    private readonly ILuaEventBridge _bridge;
    private readonly IEventBus? _bus;
    private IDisposable? _subscription;

    public LuaEventBusForwarder(ILuaEventBridge bridge, IEventBus? bus = null)
    {
        _bridge = bridge;
        _bus = bus;
    }

    /// <inheritdoc />
    public ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        if (_bus is not null)
        {
            _subscription = _bus.Subscribe<IEvent>(ForwardAsync);
        }

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask StopAsync(CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    internal static string DeriveName(Type eventType)
        => NameCache.GetOrAdd(
            eventType,
            static type =>
            {
                var name = type.Name;

                if (name.EndsWith("Event", StringComparison.Ordinal) && name.Length > "Event".Length)
                {
                    name = name[..^"Event".Length];
                }

                return name.ToSnakeCase();
            }
        );

    private static (string Key, Func<object, object?> Get)[] PayloadAccessors(Type eventType)
        => PayloadCache.GetOrAdd(
            eventType,
            static type =>
                [
                    .. type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                           .Where(property => property.CanRead && property.GetIndexParameters().Length == 0)
                           .Select(property => (property.Name.ToSnakeCase(), (Func<object, object?>)(instance => property.GetValue(instance))))
                ]
        );

    private Task ForwardAsync(IEvent eventData, CancellationToken cancellationToken)
    {
        try
        {
            var payload = new Dictionary<string, object?>(StringComparer.Ordinal);

            foreach (var (key, get) in PayloadAccessors(eventData.GetType()))
            {
                payload[key] = get(eventData);
            }

            _bridge.Publish(DeriveName(eventData.GetType()), payload);
        }
        catch (Exception ex)
        {
            Log.ForContext<LuaEventBusForwarder>()
               .Warning(ex, "Failed to forward event {EventType} to Lua", eventData.GetType().Name);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _subscription?.Dispose();
        _subscription = null;
    }
}
