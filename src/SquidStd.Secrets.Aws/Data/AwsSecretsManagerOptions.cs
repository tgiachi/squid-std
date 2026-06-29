using SquidStd.Aws.Abstractions.Data.Config;

namespace SquidStd.Secrets.Aws.Data;

/// <summary>Options for the AWS Secrets Manager store: AWS connection and an optional name prefix.</summary>
public sealed class AwsSecretsManagerOptions
{
    /// <summary>AWS-style connection config (region/credentials/endpoint override).</summary>
    public AwsConfigEntry Aws { get; set; } = new();

    /// <summary>Optional prefix applied to and stripped from secret names (e.g. <c>myapp/</c>).</summary>
    public string? NamePrefix { get; set; }
}
