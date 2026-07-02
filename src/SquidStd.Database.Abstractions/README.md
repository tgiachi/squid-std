<h1 align="center">SquidStd.Database.Abstractions</h1>

Provider-agnostic data-access contracts for SquidStd. Entities derive from `BaseEntity` (a `Guid` id
plus UTC timestamps) and are accessed through the generic `IDataAccess<TEntity>` - full CRUD, bulk
operations, paging, and composable queries - without binding to any specific ORM.

## Install

```bash
dotnet add package SquidStd.Database.Abstractions
```

## Usage

```csharp
using SquidStd.Database.Abstractions.Data.Entities;
using SquidStd.Database.Abstractions.Interfaces.Data;

public sealed class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}

// IDataAccess<User> is resolved from DI (see the SquidStd.Database package).
public async Task ExampleAsync(IDataAccess<User> users)
{
    await users.InsertAsync(new User { Name = "Ann" });
    var page = await users.GetPagedAsync(page: 1, pageSize: 20, orderBy: u => u.Name);
}
```

## Key types

| Type                   | Purpose                                          |
|------------------------|--------------------------------------------------|
| `IDataAccess<TEntity>` | CRUD + bulk + paged + composable query contract. |
| `BaseEntity`           | Guid id + UTC created/updated base entity.       |
| `PagedResultData<T>`   | Paginated result with metadata.                  |
| `DatabaseConfig`       | Connection string + auto-migrate config section. |
| `DatabaseProviderType` | Supported provider enum.                         |

## Related

- Tutorial: [Database](https://tgiachi.github.io/squid-std/tutorials/database.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
