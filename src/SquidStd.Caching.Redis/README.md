<h1 align="center">SquidStd.Caching.Redis</h1>

Redis backend for SquidStd.Caching. Implements `ICacheProvider` on top of StackExchange.Redis,
so the same `ICacheService` API reads and writes a real Redis server with native key expiry.
Registered with a single `AddRedisCache(...)` call.

## Install

```bash
dotnet add package SquidStd.Caching.Redis
```

## Usage

```csharp
using DryIoc;
using SquidStd.Caching.Abstractions.Interfaces;
using SquidStd.Caching.Redis.Extensions;

var container = new Container();
container.AddRedisCache("redis://localhost:6379?defaultTtlSeconds=300&keyPrefix=app:");

var cache = container.Resolve<ICacheService>();
await cache.SetAsync("user:1", new { Name = "squid" }, TimeSpan.FromMinutes(10));
var user = await cache.GetAsync<object>("user:1");
```

## Key types

| Type                               | Purpose                                       |
|------------------------------------|-----------------------------------------------|
| `RedisCacheRegistrationExtensions` | `AddRedisCache(...)` registration.            |
| `RedisCacheProvider`               | StackExchange.Redis-backed `ICacheProvider`.  |
| `RedisCacheOptions`                | StackExchange.Redis connection configuration. |

## Related

- Tutorial: [Caching](https://tgiachi.github.io/squid-std/tutorials/caching.html)

## License

MIT — part of [SquidStd](https://github.com/tgiachi/squid-std).
