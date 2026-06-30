using SquidStd.Aws.Abstractions.Data.Config;

namespace SquidStd.Vfs.S3.Data;

/// <summary>Connection options for the S3-compatible VFS backend (AWS / MinIO / R2 / B2).</summary>
public sealed class S3FileSystemOptions
{
    /// <summary>AWS-style connection config; <c>ServiceUrl</c> is the full endpoint for S3-compatibles.</summary>
    public AwsConfigEntry Aws { get; init; } = new();

    /// <summary>Bucket name.</summary>
    public string Bucket { get; init; } = string.Empty;
}
