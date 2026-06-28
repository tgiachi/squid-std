using System.Text;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using SquidStd.Secrets.Aws.Data;
using SquidStd.Secrets.Aws.Services;
using SquidStd.Tests.Secrets.Aws.Support;

namespace SquidStd.Tests.Secrets.Aws;

[Collection(LocalStackSecretsCollection.Name)]
public class KmsSecretProtectorTests
{
    private readonly LocalStackSecretsFixture _localStack;

    public KmsSecretProtectorTests(LocalStackSecretsFixture localStack)
    {
        _localStack = localStack;
    }

    [Fact]
    public async Task Protect_Unprotect_RoundTripsLargePayload_WithoutPlaintext()
    {
        var keyId = await CreateKeyAsync();
        var protector = new KmsSecretProtector(new KmsSecretProtectorOptions { Aws = _localStack.Aws, KeyId = keyId });
        var plaintext = Encoding.UTF8.GetBytes(new string('s', 10_000)); // > 4 KB → exercises envelope

        var blob = protector.Protect(plaintext);
        var roundTrip = protector.Unprotect(blob);

        Assert.Equal(plaintext, roundTrip);
        Assert.DoesNotContain("ssss", Encoding.UTF8.GetString(blob), StringComparison.Ordinal);
    }

    private async Task<string> CreateKeyAsync()
    {
        using var kms = new AmazonKeyManagementServiceClient(
            new Amazon.Runtime.BasicAWSCredentials("test", "test"),
            new AmazonKeyManagementServiceConfig
                { ServiceURL = _localStack.Aws.ServiceUrl, AuthenticationRegion = "us-east-1" }
        );
        var created = await kms.CreateKeyAsync(new CreateKeyRequest());

        return created.KeyMetadata.KeyId;
    }
}
