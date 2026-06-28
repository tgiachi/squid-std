using SquidStd.Aws.Abstractions.Data.Config;

namespace SquidStd.Secrets.Aws.Data;

/// <summary>Options for the KMS-backed secret protector: AWS connection and the KMS key id/alias.</summary>
public sealed class KmsSecretProtectorOptions
{
    /// <summary>AWS-style connection config (region/credentials/endpoint override).</summary>
    public AwsConfigEntry Aws { get; set; } = new();

    /// <summary>The KMS key id or alias used to generate data keys (e.g. <c>alias/app</c>).</summary>
    public string KeyId { get; set; } = string.Empty;
}
