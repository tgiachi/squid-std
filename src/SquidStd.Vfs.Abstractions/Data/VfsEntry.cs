namespace SquidStd.Vfs.Abstractions.Data;

/// <summary>An entry listed in a virtual filesystem: its logical path, byte size, and last-modified time.</summary>
public sealed record VfsEntry(string Path, long Size, DateTimeOffset ModifiedUtc);
