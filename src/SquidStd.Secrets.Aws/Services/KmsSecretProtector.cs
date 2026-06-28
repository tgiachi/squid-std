using System.Security.Cryptography;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using SquidStd.Core.Interfaces.Secrets;
using SquidStd.Secrets.Aws.Data;
using SquidStd.Secrets.Aws.Internal;

namespace SquidStd.Secrets.Aws.Services;

/// <summary>An <see cref="ISecretProtector" /> that envelope-encrypts with AWS KMS data keys.</summary>
public sealed class KmsSecretProtector : ISecretProtector, IDisposable
{
    private readonly AmazonKeyManagementServiceClient _kms;
    private readonly string _keyId;

    public KmsSecretProtector(KmsSecretProtectorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.KeyId);

        _keyId = options.KeyId;
        _kms = new AmazonKeyManagementServiceClient(
            AwsClientFactory.Credentials(options.Aws),
            AwsClientFactory.KmsConfig(options.Aws)
        );
    }

    /// <inheritdoc />
    public byte[] Protect(byte[] plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext);

        var generated = _kms.GenerateDataKeyAsync(
                new GenerateDataKeyRequest { KeyId = _keyId, KeySpec = DataKeySpec.AES_256 }
            )
            .GetAwaiter()
            .GetResult();

        var dataKey = generated.Plaintext.ToArray();

        try
        {
            return KmsEnvelope.Seal(dataKey, generated.CiphertextBlob.ToArray(), plaintext);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(dataKey);
        }
    }

    /// <inheritdoc />
    public byte[] Unprotect(byte[] protectedData)
    {
        ArgumentNullException.ThrowIfNull(protectedData);

        var wrappedKey = KmsEnvelope.ReadWrappedKey(protectedData);
        var decrypted = _kms.DecryptAsync(
                new DecryptRequest { CiphertextBlob = new MemoryStream(wrappedKey) }
            )
            .GetAwaiter()
            .GetResult();

        var dataKey = decrypted.Plaintext.ToArray();

        try
        {
            return KmsEnvelope.Open(dataKey, protectedData);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(dataKey);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _kms.Dispose();
    }
}
