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

## Seeding

Seeders populate initial database data once per seeder name. Each seeder's name is tracked in the
`__squidstd_seed_history` table; a seeder runs only if its name does not yet exist in the history. The
history row is written only after a successful run - if a seeder throws, it is retried at the next
process start. A seeder exception aborts startup (fail-fast). The seed and history row are not wrapped
in a transaction, so a seeder should tolerate running again over data it already wrote.

The history table is created automatically even if `AutoMigrate` is off. Seeder names must be unique
and are checked case-insensitively (the lookup follows database collation, but the uniqueness check is
ordinal).

### Delegate seeder

Register an inline seeding callback with a unique name:

```csharp
bootstrap.ConfigureServices(c =>
{
    c.RegisterDatabase();  // binds "database" config, runs seeders after init
    
    // Inline delegate seeder
    c.RegisterDatabaseSeeder("accounts.admin", async (database, ct) =>
    {
        var users = database.Resolve<IDataAccess<User>>();
        await users.InsertAsync(new User { Id = 1, Name = "Admin" }, ct);
    });
    
    return c;
});
```

### Class seeder

Implement `IDatabaseSeeder` (which provides the name) and register it by type:

```csharp
public sealed class AdminAccountSeeder : IDatabaseSeeder
{
    public string Name => "accounts.admin";
    
    public async ValueTask SeedAsync(IDatabaseService database, CancellationToken cancellationToken = default)
    {
        var users = database.Resolve<IDataAccess<User>>();
        await users.InsertAsync(new User { Id = 1, Name = "Admin" }, cancellationToken);
    }
}

bootstrap.ConfigureServices(c =>
{
    c.RegisterDatabase();
    c.RegisterDatabaseSeeder<AdminAccountSeeder>();
    
    return c;
});
```

### Key semantics

- **Run-once per name**: The `__squidstd_seed_history` table tracks which seeders have successfully run.
  A seeder is skipped if its name already exists in the history.
- **Failed-seeder retry**: The history row is written only after the seeder succeeds. If a seeder throws,
  the row is not written, and the seeder retries at the next process start.
- **History table creation**: The table is created automatically during database service initialization,
  even if `AutoMigrate` is false. Its schema is internal to SquidStd and should not be modified.
- **Duplicate name check**: Seeder names must be unique. Duplicate names (case-insensitive match) are
  detected at startup and cause an exception.
- **Execution order**: Seeders run in registration order. Multiple seeders can be registered via chained
  `RegisterDatabaseSeeder()` calls.
- **No transaction wrap**: The seed and its history row are not wrapped in a transaction. Seeders should
  be idempotent or accept re-running over partially-written state.

## Related

- Tutorial: [Database](https://tgiachi.github.io/squid-std/tutorials/database.html)

## License

MIT - part of [SquidStd](https://github.com/tgiachi/squid-std).
