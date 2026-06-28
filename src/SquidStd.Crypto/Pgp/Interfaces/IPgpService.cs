using SquidStd.Crypto.Pgp.Data;

namespace SquidStd.Crypto.Pgp.Interfaces;

/// <summary>
///     OpenPGP operations over the keyring: key generation, encrypt/decrypt, sign/verify, and the combined
///     encrypt+sign / decrypt+verify flows. Recipients and signers are resolved from the keyring by identity.
/// </summary>
public interface IPgpService
{
    /// <summary>Generates a new key pair for <paramref name="identity" /> protected by <paramref name="passphrase" />.</summary>
    PgpKey GenerateKey(string identity, string passphrase, PgpKeyOptions? options = null);

    /// <summary>Encrypts <paramref name="data" /> for the recipient, returning an armored message.</summary>
    Task<string> EncryptForAsync(string recipientIdentity, byte[] data, CancellationToken cancellationToken = default);

    /// <summary>Encrypts a stream for the recipient, writing an armored message to <paramref name="output" />.</summary>
    Task EncryptForAsync(
        string recipientIdentity, Stream input, Stream output, CancellationToken cancellationToken = default
    );

    /// <summary>Decrypts an armored message using the matching keyring secret key and passphrase.</summary>
    Task<byte[]> DecryptAsync(string armored, string passphrase, CancellationToken cancellationToken = default);

    /// <summary>Decrypts a stream using the matching keyring secret key and passphrase.</summary>
    Task DecryptAsync(Stream input, Stream output, string passphrase, CancellationToken cancellationToken = default);

    /// <summary>Encrypts <paramref name="data" /> for the recipient and signs it with the signer's secret key.</summary>
    Task<string> EncryptAndSignForAsync(
        string recipientIdentity, byte[] data, string signerIdentity, string signerPassphrase,
        CancellationToken cancellationToken = default
    );

    /// <summary>Encrypts and signs a stream for the recipient.</summary>
    Task EncryptAndSignForAsync(
        string recipientIdentity, Stream input, Stream output, string signerIdentity, string signerPassphrase,
        CancellationToken cancellationToken = default
    );

    /// <summary>Decrypts an armored message and reports whether it was signed and whether the signature validated.</summary>
    Task<PgpDecryptionResult> DecryptAndVerifyAsync(
        string armored, string passphrase, CancellationToken cancellationToken = default
    );

    /// <summary>Produces an armored signed message that embeds <paramref name="data" />.</summary>
    Task<string> SignAsync(
        byte[] data, string signerIdentity, string passphrase, CancellationToken cancellationToken = default
    );

    /// <summary>Verifies an armored signed message against the keyring, recovering the embedded content.</summary>
    Task<PgpVerificationResult> VerifyAsync(string signedMessage, CancellationToken cancellationToken = default);
}
