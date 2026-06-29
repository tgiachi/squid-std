# Testing

For fast, isolated tests, bootstrap SquidStd with in-memory providers instead of
real infrastructure. The in-memory backends implement the same abstractions as
the production providers, so the code under test never changes between unit and
integration runs.

## Steps

1. **Build a fixture** that creates a `SquidStdBootstrap` and starts it once per
   test class.
2. **Opt into in-memory providers** in `ConfigureServices`: `AddInMemoryCache`,
   `AddInMemoryMessaging`, and an in-memory VFS via `RegisterVfs` with
   `InMemoryFileSystem`.
3. **Resolve abstractions** (`ICacheService`, the event bus, `IVirtualFileSystem`, …)
   from the container in your tests.
4. **Swap to real backends** in a separate integration suite by replacing the
   `Add…` calls with `AddRedisCache`, `AddRabbitMqMessaging`, `AddS3Storage`, etc.

```csharp
public sealed class TestHostFixture : IAsyncDisposable
{
    public ISquidStdBootstrap Bootstrap { get; }

    public TestHostFixture()
    {
        Bootstrap = SquidStdBootstrap.Create(
            new SquidStdOptions { ConfigName = "squidstd", RootDirectory = AppContext.BaseDirectory });

        Bootstrap.ConfigureServices(container => container
            .AddInMemoryCache()
            .AddInMemoryMessaging()
            .RegisterVfs(_ => new InMemoryFileSystem()));
    }

    public async ValueTask DisposeAsync() => await Bootstrap.StopAsync();
}
```

Start the bootstrap (`await Bootstrap.StartAsync()`) before your first test and
resolve services from its container.
