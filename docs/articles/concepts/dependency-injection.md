# Dependency injection

SquidStd resolves every service through dependency injection. Understanding the container and its registration pattern is the key to wiring an application together.

## DryIoc container

The DI container is [DryIoc](https://github.com/dadhi/DryIoc), exposed as `IContainer`. It is fast, supports rich lifetimes, and validates required services at resolution time. Because the container owns validation, constructor dependencies that arrive from DI do not need manual null guards.

## The AddXxx / RegisterXxx extension pattern

Modules register themselves through C# 14 `extension(IContainer)` members named `AddXxx(...)` or `RegisterXxx(...)`. Each capability ships its own registration entry point, so wiring a module is a single call:

```csharp
container.AddInMemoryMessaging();
container.RegisterCoreServices();
```

This keeps registration discoverable and colocated with the package that owns it.

## Resolving through the bootstrap

Once configured, resolve services through the bootstrap:

```csharp
var bus = bootstrap.Resolve<IEventBus>();
```

In most code you let constructor injection do the work and never call `Resolve` directly. See [bootstrap lifecycle](bootstrap-lifecycle.md) for where `ConfigureServices` runs.

## Singletons and lifecycle

Most infrastructure services are registered as singletons and live for the life of the host. Services implementing `ISquidStdService` are additionally started and stopped by the bootstrap, so a singleton can hold long-lived resources that are released on shutdown.
