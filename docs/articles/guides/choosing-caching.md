# Choosing a cache

The caching abstraction (`ICacheService`) has two backends. Both expose the same
API, so you can develop against in-memory and promote to Redis without code changes.

| Backend | Package · entrypoint | Use case | Scope | Survives restart | Ops cost |
|---|---|---|---|---|---|
| In-memory | `SquidStd.Caching` · `AddInMemoryCache` | Tests, single instance | Per-process | No | None |
| Redis | `SquidStd.Caching.Redis` · `AddRedisCache` | Multiple instances, shared cache | Distributed | Yes (with persistence) | Run/host Redis |

```csharp
bootstrap.ConfigureServices(container => container.AddRedisCache("localhost:6379"));
```

## Recommendation

Use `AddInMemoryCache` for tests and single-instance deployments; switch to
`AddRedisCache` as soon as you run more than one instance or need the cache to
outlive the process.
