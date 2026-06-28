<h1 align="center">SquidStd.Database</h1>

FreeSql-backed implementation of the SquidStd.Database.Abstractions contracts. It owns a singleton
`IFreeSql`, exposes a generic `FreeSqlDataAccess<TEntity>` with transactional writes (rollback on
failure), bulk operations and paging, parses URI-style connection strings, and can auto-sync the schema
on startup.

## Install

```bash
dotnet add package SquidStd.Database
```

## Usage

```csharp
using DryIoc;
using SquidStd.Database.Abstractions.Interfaces.Data;
using SquidStd.Database.Extensions;

var container = new Container();
container.RegisterDatabase(); // reads the "database" config section

// DatabaseConfig: ConnectionString = "postgres://user:pass@host:5432/app", AutoMigrate = true
var users = container.Resolve<IDataAccess<User>>();
await users.InsertAsync(new User { Name = "Ann" });
var page = await users.GetPagedAsync(page: 1, pageSize: 20, orderBy: u => u.Name);
```

## Key types

| Type                         | Purpose                                                             |
|------------------------------|---------------------------------------------------------------------|
| `RegisterDatabaseExtension`  | `RegisterDatabase()` DI registration.                               |
| `DatabaseService`            | Owns the singleton `IFreeSql`; builds it and (optionally) migrates. |
| `FreeSqlDataAccess<TEntity>` | FreeSql `IDataAccess<TEntity>` implementation.                      |
| `ConnectionStringParser`     | URI → provider + native connection string.                          |
| `ZLinqResultExtensions`      | Zero-alloc in-memory result helpers.                                |

## Related

- Tutorial: [Database](https://tgiachi.github.io/squid-std/tutorials/database.html)

## License

MIT — part of [SquidStd](https://github.com/tgiachi/squid-std).
