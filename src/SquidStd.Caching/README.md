<h1 align="center">SquidStd.Caching</h1>

In-memory backend for SquidStd.Caching. Provides an `IMemoryCache`-backed `ICacheProvider` with
absolute TTL and eviction, wired to the shared typed `ICacheService` facade - registered with a
single `AddInMemoryCache()` call. Ideal for single-process apps, tests, and local dev.

## Install

```bash
dotnet add package SquidStd.Caching
```

## Usage

```csharp
using DryIoc;
using SquidStd.Caching.Abstractions.Interfaces;
using SquidStd.Caching.Extensions;

var container = new Container();
container.AddInMemoryCache("memory://localhost?defaultTtlSeconds=300&keyPrefix=app:");

var cache = container.Resolve<ICacheService>();
await cache.SetAsync("user:1", new { Name = "squid" });
var user = await cache.GetAsync<object>("user:1");
```

## Key types

| Type                          | Purpose                                 |
|-------------------------------|-----------------------------------------|
| `CacheRegistrationExtensions` | `AddInMemoryCache(...)` registration.   |
| `InMemoryCacheProvider`       | `IMemoryCache`-backed `ICacheProvider`. |

## Related

- Tutorial: [Caching](https://tgiachi.github.io/squid-std/tutorials/caching.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
