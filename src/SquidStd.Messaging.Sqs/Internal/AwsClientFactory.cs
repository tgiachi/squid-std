using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using SquidStd.Aws.Abstractions.Data.Config;

namespace SquidStd.Messaging.Sqs.Internal;

/// <summary>
/// Builds AWS SDK clients/credentials from a shared <see cref="AwsConfigEntry" />.
/// </summary>
internal static class AwsClientFactory
{
    public static AWSCredentials Credentials(AwsConfigEntry aws)
    {
        if (!string.IsNullOrWhiteSpace(aws.AccessKey) && !string.IsNullOrWhiteSpace(aws.SecretKey))
        {
            return string.IsNullOrWhiteSpace(aws.SessionToken)
                       ? new BasicAWSCredentials(aws.AccessKey, aws.SecretKey)
                       : new SessionAWSCredentials(aws.AccessKey, aws.SecretKey, aws.SessionToken);
        }

        return FallbackCredentialsFactory.GetCredentials();
    }

    public static AmazonSimpleNotificationServiceConfig SnsConfig(AwsConfigEntry aws)
    {
        var config = new AmazonSimpleNotificationServiceConfig();
        Configure(config, aws);

        return config;
    }

    public static AmazonSQSConfig SqsConfig(AwsConfigEntry aws)
    {
        var config = new AmazonSQSConfig();
        Configure(config, aws);

        return config;
    }

    private static void Configure(ClientConfig config, AwsConfigEntry aws)
    {
        if (!string.IsNullOrWhiteSpace(aws.ServiceUrl))
        {
            config.ServiceURL = aws.ServiceUrl;
            config.AuthenticationRegion = aws.Region;
        }
        else
        {
            config.RegionEndpoint = RegionEndpoint.GetBySystemName(aws.Region);
        }
    }
}
