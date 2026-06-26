using SquidStd.Aws.Abstractions.Data.Config;

namespace SquidStd.Messaging.Sqs.Data.Config;

/// <summary>
///     Configuration for the SQS/SNS messaging provider. Connection details live in <see cref="Aws" />;
///     the remaining knobs tune SQS receive behaviour.
/// </summary>
public sealed class SqsOptions
{
    /// <summary>AWS connection config (region, credentials, endpoint override).</summary>
    public AwsConfigEntry Aws { get; init; } = new();

    /// <summary>Messages requested per ReceiveMessage call (1..10). Default 10.</summary>
    public int MaxNumberOfMessages { get; init; } = 10;

    /// <summary>Visibility timeout applied to received messages. Default 30 seconds.</summary>
    public TimeSpan VisibilityTimeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>Long-poll wait time in seconds (0..20). Default 20.</summary>
    public int WaitTimeSeconds { get; init; } = 20;
}
