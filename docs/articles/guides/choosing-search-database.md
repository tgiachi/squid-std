# Choosing search & database

Full-text search and relational persistence are different tools. Use the search
provider for queries over text and documents, and the database module (backed by
FreeSql) for structured, relational data.

| Need | Module · entrypoint | Provider(s) | Use case |
|---|---|---|---|
| Full-text / document search | `SquidStd.Search.Elasticsearch` · `AddElasticsearch` | Elasticsearch | Relevance ranking, faceting, log/document search |
| Relational data | `SquidStd.Database` · `RegisterDatabase` | FreeSql: Sqlite, PostgreSQL, MySql, SqlServer | Transactional records, joins, constraints |

The database provider is selected via `DatabaseProviderType` (`Sqlite`,
`PostgreSQL`, `MySql`, `SqlServer`) in the `database` config section.

```csharp
bootstrap.ConfigureServices(container => container.RegisterDatabase());
```

## Recommendation

Reach for `AddElasticsearch` when the core requirement is searching text or
documents by relevance; use `RegisterDatabase` with the FreeSql provider that
matches your engine for everything relational. They compose — many apps use both.
