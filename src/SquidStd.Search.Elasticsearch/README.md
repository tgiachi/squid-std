<h1 align="center">SquidStd.Search.Elasticsearch</h1>

Elasticsearch provider for SquidStd.Search. Indexes `IIndexableEntity` documents and exposes a constrained
LINQ `IQueryable<T>` translated to the Elasticsearch query DSL; the native `ElasticsearchClient` is registered
for advanced queries.

## Install

```bash
dotnet add package SquidStd.Search.Elasticsearch
```

## Usage

```csharp
using DryIoc;
using SquidStd.Search.Abstractions.Interfaces;
using SquidStd.Search.Elasticsearch.Data.Config;
using SquidStd.Search.Elasticsearch.Extensions;
using SquidStd.Search.Elasticsearch.Linq;

container.AddElasticsearch(new ElasticsearchOptions { Uri = new Uri("http://localhost:9200") });

var search = container.Resolve<ISearchService>();
await search.IndexAsync(new Order("1", "open", 150), refresh: true);

var open = await search.Query<Order>()
    .Where(o => o.Status == "open" && o.Total > 100)
    .OrderByDescending(o => o.Total)
    .Take(20)
    .ToListAsync();
```

Supported LINQ: `Where` (`==`, `!=`, `<`, `>`, `<=`, `>=`, `&&`, `||`, `!`, `string.Contains`/`StartsWith`,
bool members), `OrderBy`/`ThenBy`(`Descending`), `Skip`/`Take`, and `.Match(field, text)` / `.FullText(text)`.
Anything else throws `NotSupportedException` — drop down to the native `ElasticsearchClient`.

## Key types

| Type                           | Purpose                                                                |
|--------------------------------|------------------------------------------------------------------------|
| `SearchRegistrationExtensions` | `AddElasticsearch(...)` registration.                                  |
| `ElasticSearchService`         | `ISearchService` backed by the Elasticsearch client.                   |
| `ElasticsearchOptions`         | Connection options (URI, credentials).                                 |
| `ElasticQueryable`             | Constrained `IQueryable<T>` translated to the Elasticsearch query DSL. |

## Related

- Tutorial: [Search](https://tgiachi.github.io/squid-std/tutorials/search.html)

## License

MIT — part of [SquidStd](https://github.com/tgiachi/squid-std).
