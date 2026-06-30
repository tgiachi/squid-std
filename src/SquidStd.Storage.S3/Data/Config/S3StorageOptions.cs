using SquidStd.Aws.Abstractions.Data.Config;

namespace SquidStd.Storage.S3.Data.Config;

/// <summary>
/// Connection options for the S3-compatible (MinIO) storage provider. Connection details live in
/// <see cref="Aws" /> (shared with other AWS-SDK providers); <see cref="Bucket" /> is storage-specific.
/// </summary>
public sealed class S3StorageOptions
{
    /// <summary>AWS-style connection config. <c>ServiceUrl</c> is the full endpoint URL (http/https).</summary>
    public AwsConfigEntry Aws { get; init; } = new();

    /// <summary>Bucket name.</summary>
    public string Bucket { get; init; } = string.Empty;
}
