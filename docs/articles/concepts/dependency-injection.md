# Dependency injection

SquidStd resolves every service through dependency injection. Understanding the container and its registration pattern is the key to wiring an application together.

## DryIoc container

The DI container is [DryIoc](https://github.com/dadhi/DryIoc), exposed as `IContainer`. It is fast, supports rich lifetimes, and validates required services at resolution time. Because the container owns validation, constructor dependencies that arrive from DI do not need manual null guards.

## The AddXxx / RegisterXxx extension pattern

Modules register themselves through C# 14 `extension(IContainer)` members named `AddXxx(...)` or `RegisterXxx(...)`. Each capability ships its own registration entry point, so wiring a module is a single call:

```csharp
bootstrap.ConfigureServices(container => container
    .RegisterCoreServices()
    .AddInMemoryMessaging());
```

This keeps registration discoverable and colocated with the package that owns it.

`RegisterCoreServices` comes in two flavors:

- **Parameterless** - registers the core services only (JSON serializer, event bus, job system, main-thread dispatcher, timer wheel, metrics collection, secrets). Use it inside `ConfigureServices` after the bootstrap is created, since the bootstrap already provides the configuration core. Granular methods such as `RegisterEventBusService()` or `RegisterTimerWheelService()` let you pick individual services instead.
- **`RegisterCoreServices(configName, configDirectory)`** - registers the configuration core (directories, `logger` section, config manager) plus all core services. Use it on a standalone container without a bootstrap:

```csharp
var container = new Container();
container.RegisterCoreServices("myapp", Directory.GetCurrentDirectory());
```

## Resolving through the bootstrap

Once configured, resolve services through the bootstrap:

```csharp
var bus = bootstrap.Resolve<IEventBus>();
```

In most code you let constructor injection do the work and never call `Resolve` directly. See [bootstrap lifecycle](bootstrap-lifecycle.md) for where `ConfigureServices` runs.

## Singletons and lifecycle

Most infrastructure services are registered as singletons and live for the life of the host. Services implementing `ISquidStdService` are additionally started and stopped by the bootstrap, so a singleton can hold long-lived resources that are released on shutdown.
