using FreeSql.DataAnnotations;
using SquidStd.Database.Abstractions.Data.Entities;

namespace SquidStd.Vfs.Database.Data.Entities;

/// <summary>A stored file keyed by its logical VFS path.</summary>
[Index("uq_vfs_file_path", "Path", true)]
public sealed class VfsFileEntity : BaseEntity
{
    /// <summary>The logical VFS path (unique).</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>The file bytes.</summary>
    public byte[] Content { get; set; } = [];

    /// <summary>The file size in bytes.</summary>
    public long Size { get; set; }
}
