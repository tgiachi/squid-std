namespace SquidStd.Vfs.Internal;

/// <summary>Normalizes logical VFS paths to forward-slash, root-relative form and rejects traversal.</summary>
internal static class VfsPath
{
    public static string Normalize(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var segments = path.Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var segment in segments)
        {
            if (segment is "." or "..")
            {
                throw new ArgumentException($"Path '{path}' must not contain relative segments.", nameof(path));
            }
        }

        return string.Join('/', segments);
    }
}
