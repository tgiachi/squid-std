<h1 align="center">SquidStd.Caching</h1>

In-memory backend for SquidStd.Caching. Provides an `IMemoryCache`-backed `ICacheProvider` with absolute
TTL and eviction, wired to the shared typed `ICacheService` facade - registered with a single
`AddInMemoryCache()` call.

The contract lives in `SquidStd.Caching.Abstractions`, so code written against `ICacheService` moves
unchanged to a distributed cache: use this in-memory provider for single-process apps, tests, and local
dev, then swap in `SquidStd.Caching.Redis` for production. Values are serialized as JSON in both cases,
so cached shapes behave the same across providers.

## Install

```bash
dotnet add package SquidStd.Caching
```

## Usage

Register through the bootstrap - the provider is an `ISquidStdService`, so `StartAsync` / `StopAsync`
manage its lifecycle for you:

```csharp
using SquidStd.Caching.Abstractions.Interfaces;
using SquidStd.Caching.Extensions;
using SquidStd.Services.Core.Extensions;
using SquidStd.Services.Core.Services.Bootstrap;

var bootstrap = SquidStdBootstrap.Create(o => o.ConfigName = "myapp");
bootstrap.ConfigureServices(c => c.RegisterCoreServices().AddInMemoryCache());
await bootstrap.StartAsync();
```

### Get / set with TTL

```csharp
var cache = bootstrap.Resolve<ICacheService>();

await cache.SetAsync("greeting", "hello", TimeSpan.FromMinutes(5));
var value = await cache.GetAsync<string>("greeting");   // "hello", or default on a miss

var exists = await cache.ExistsAsync("greeting");        // true
var removed = await cache.RemoveAsync("greeting");       // true if the key existed
```

Omitting the TTL on `SetAsync` falls back to `CacheOptions.DefaultTtl` (no expiry when that is `null`).

### Cache-aside with `GetOrSetAsync`

`GetOrSetAsync` returns the cached value, or runs the factory, stores the result, and returns it on a
miss - the standard cache-aside pattern in one call:

```csharp
public sealed record UserProfile(string Id, string Name);

var profile = await cache.GetOrSetAsync(
    $"user:{id}",
    async ct => await LoadProfileFromDatabaseAsync(id, ct),
    TimeSpan.FromMinutes(10)
);
```

## Configuration

`AddInMemoryCache` takes an optional `CacheOptions`. Options are code-only - this package registers no
YAML config section.

```csharp
c.AddInMemoryCache(new CacheOptions
{
    DefaultTtl = TimeSpan.FromMinutes(5),
    KeyPrefix = "app:"
});
```

| Property     | Default        | Purpose                                                          |
|--------------|----------------|------------------------------------------------------------------|
| `DefaultTtl` | `null`         | TTL applied when a set call passes none; `null` means no expiry. |
| `KeyPrefix`  | `string.Empty` | Prefix prepended to every key.                                   |

A connection-string overload is also available:
`AddInMemoryCache("memory://local?defaultTtlSeconds=300&keyPrefix=app:")`.

`CacheMetricsProvider` is registered as `ICacheMetrics` / `IMetricProvider` and exposes aggregate hit,
miss, set, and remove counters plus the hit ratio to the metrics collection system.

## Key types

| Type                          | Purpose                                                    |
|-------------------------------|------------------------------------------------------------|
| `CacheRegistrationExtensions` | `AddInMemoryCache(...)` registration.                      |
| `InMemoryCacheProvider`       | `IMemoryCache`-backed `ICacheProvider`.                    |
| `ICacheService`               | Typed facade from `SquidStd.Caching.Abstractions`: get/set/TTL, `GetOrSetAsync`, exists/remove. |
| `CacheOptions`                | Default TTL and key prefix configuration.                  |

## Related

- Article: [Caching](https://tgiachi.github.io/squid-std/articles/caching.html)
- Tutorial: [Caching](https://tgiachi.github.io/squid-std/tutorials/caching.html)
- Production backend: [SquidStd.Caching.Redis](https://tgiachi.github.io/squid-std/articles/caching-redis.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
