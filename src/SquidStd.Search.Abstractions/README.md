<h1 align="center">SquidStd.Search.Abstractions</h1>

Search/indexing contracts for SquidStd: tag entities with `IIndexableEntity` + `[SearchIndex]` (with
`${VAR}` / `${VAR:-default}` environment expansion) and query them via `ISearchService`.

## Install

```bash
dotnet add package SquidStd.Search.Abstractions
```

## Key types

| Type                      | Purpose                                                        |
|---------------------------|----------------------------------------------------------------|
| `IIndexableEntity`        | Marks an entity indexable and supplies its `IndexId`.          |
| `SearchIndexAttribute`    | Declares the index name (env-expanded).                        |
| `ISearchService`          | Index / delete / ensure-index / `Query<T>()`.                  |
| `SearchIndexNameResolver` | Resolves the index name from the attribute/type + environment. |

## Related

- Tutorial: [Search](https://tgiachi.github.io/squid-std/tutorials/search.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
