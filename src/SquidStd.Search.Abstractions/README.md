<p align="center">
  <img src="https://raw.githubusercontent.com/tgiachi/squid-std/main/assets/icon.png" alt="SquidStd" width="120" height="120" />
</p>

<h1 align="center">SquidStd.Search.Abstractions</h1>

<p align="center">
  <a href="https://www.nuget.org/packages/SquidStd.Search.Abstractions/"><img src="https://img.shields.io/nuget/v/SquidStd.Search.Abstractions.svg" alt="NuGet" /></a>
  <img src="https://img.shields.io/nuget/dt/SquidStd.Search.Abstractions.svg" alt="Downloads" />
  <a href="https://tgiachi.github.io/squid-std/articles/search-abstractions.html"><img src="https://img.shields.io/badge/docs-DocFX-1390A3.svg" alt="docs" /></a>
  <img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="license" />
</p>

Search/indexing contracts for SquidStd: tag entities with `IIndexableEntity` + `[SearchIndex]` (with
`${VAR}` / `${VAR:-default}` environment expansion) and query them via `ISearchService`.

## Install

```bash
dotnet add package SquidStd.Search.Abstractions
```

## Key types

| Type | Purpose |
|------|---------|
| `IIndexableEntity` | Marks an entity indexable and supplies its `IndexId`. |
| `SearchIndexAttribute` | Declares the index name (env-expanded). |
| `ISearchService` | Index / delete / ensure-index / `Query<T>()`. |
| `SearchIndexNameResolver` | Resolves the index name from the attribute/type + environment. |

## License

MIT — part of [SquidStd](https://github.com/tgiachi/squid-std).
