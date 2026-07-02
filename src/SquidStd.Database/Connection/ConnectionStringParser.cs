using System.Globalization;
using SquidStd.Database.Abstractions.Types.Data;

namespace SquidStd.Database.Connection;

/// <summary>
/// Parses URI-style connection strings ("scheme://...") into a provider and native connection string.
/// </summary>
public static class ConnectionStringParser
{
    /// <summary>
    /// Parses the given URI connection string.
    /// </summary>
    /// <param name="connectionString">The URI connection string.</param>
    /// <param name="sqliteBaseDirectory">Base directory used to resolve relative SQLite file paths; null keeps them verbatim.</param>
    /// <returns>The parsed provider and native connection string.</returns>
    public static ParsedConnection Parse(string connectionString, string? sqliteBaseDirectory = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var schemeEnd = connectionString.IndexOf("://", StringComparison.Ordinal);

        if (schemeEnd <= 0)
        {
            throw new FormatException($"Connection string '{connectionString}' is not a valid URI.");
        }

        var scheme = connectionString[..schemeEnd].ToLowerInvariant();
        var remainder = connectionString[(schemeEnd + 3)..];
        var provider = ResolveProvider(scheme);

        if (provider == DatabaseProviderType.Sqlite)
        {
            var filePath = ResolveSqlitePath(remainder, sqliteBaseDirectory);

            return new(provider, $"Data Source={filePath ?? remainder}", filePath);
        }

        return new(provider, BuildServer(provider, remainder));
    }

    private static string BuildServer(DatabaseProviderType provider, string remainder)
    {
        // Split "[user[:pass]@]host[:port]/database[?query]" into authority and path.
        var slash = remainder.IndexOf('/', StringComparison.Ordinal);

        if (slash < 0)
        {
            throw new FormatException("Connection string requires a database name.");
        }

        var authority = remainder[..slash];
        var pathAndQuery = remainder[(slash + 1)..];

        var query = pathAndQuery.IndexOf('?', StringComparison.Ordinal);
        var database = (query < 0 ? pathAndQuery : pathAndQuery[..query]).Trim('/');

        if (string.IsNullOrEmpty(database))
        {
            throw new FormatException("Connection string requires a database name.");
        }

        var (user, password, hostPort) = SplitAuthority(authority);
        var (host, port) = SplitHostPort(hostPort);

        if (string.IsNullOrEmpty(host))
        {
            throw new FormatException("Connection string requires a host.");
        }

        return provider switch
        {
            DatabaseProviderType.Postgres =>
                $"Host={host};Port={port ?? 5432};Username={user};Password={password};Database={database}",
            DatabaseProviderType.MySql =>
                $"Server={host};Port={port ?? 3306};Uid={user};Pwd={password};Database={database}",
            DatabaseProviderType.SqlServer =>
                $"Server={host},{port ?? 1433};User Id={user};Password={password};Database={database};TrustServerCertificate=true",
            _ => throw new NotSupportedException($"Unsupported provider {provider}.")
        };
    }

    private static string? ResolveSqlitePath(string remainder, string? baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(remainder))
        {
            throw new FormatException("SQLite connection string requires a file path or ':memory:'.");
        }

        if (string.Equals(remainder, ":memory:", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!Path.IsPathRooted(remainder) && !string.IsNullOrWhiteSpace(baseDirectory))
        {
            return Path.GetFullPath(Path.Combine(baseDirectory, remainder));
        }

        return remainder;
    }

    private static DatabaseProviderType ResolveProvider(string scheme)
        => scheme switch
        {
            "sqlite"                   => DatabaseProviderType.Sqlite,
            "postgres" or "postgresql" => DatabaseProviderType.Postgres,
            "sqlserver" or "mssql"     => DatabaseProviderType.SqlServer,
            "mysql"                    => DatabaseProviderType.MySql,
            _                          => throw new NotSupportedException($"Unsupported database scheme '{scheme}'.")
        };

    private static (string User, string Password, string HostPort) SplitAuthority(string authority)
    {
        var at = authority.LastIndexOf('@');

        if (at < 0)
        {
            return (string.Empty, string.Empty, authority);
        }

        var userInfo = authority[..at];
        var hostPort = authority[(at + 1)..];
        var separator = userInfo.IndexOf(':', StringComparison.Ordinal);

        if (separator < 0)
        {
            return (Uri.UnescapeDataString(userInfo), string.Empty, hostPort);
        }

        var user = Uri.UnescapeDataString(userInfo[..separator]);
        var password = Uri.UnescapeDataString(userInfo[(separator + 1)..]);

        return (user, password, hostPort);
    }

    private static (string Host, int? Port) SplitHostPort(string hostPort)
    {
        var separator = hostPort.IndexOf(':', StringComparison.Ordinal);

        if (separator < 0)
        {
            return (hostPort, null);
        }

        var host = hostPort[..separator];
        var port = int.Parse(hostPort[(separator + 1)..], CultureInfo.InvariantCulture);

        return (host, port);
    }
}
