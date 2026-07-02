using SquidStd.Database.Abstractions.Types.Data;

namespace SquidStd.Database.Connection;

/// <summary>
/// The result of parsing a URI connection string: the provider and the native connection string.
/// </summary>
/// <param name="Provider">The resolved database provider.</param>
/// <param name="NativeConnectionString">The provider-native connection string for FreeSql.</param>
/// <param name="SqliteFilePath">The resolved on-disk SQLite file path, or null for :memory: and server providers.</param>
public sealed record ParsedConnection(
    DatabaseProviderType Provider,
    string NativeConnectionString,
    string? SqliteFilePath = null
);
