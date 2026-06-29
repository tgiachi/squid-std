using SquidStd.Crypto.Pgp.Data;

namespace SquidStd.Crypto.Pgp.Interfaces;

/// <summary>
///     In-memory collection of PGP keys, indexed by identity, key id, and fingerprint. Crypto operations look
///     recipients and signers up here by identity.
/// </summary>
public interface IPgpKeyring
{
    /// <summary>All keys currently held.</summary>
    IReadOnlyCollection<PgpKey> Keys { get; }

    /// <summary>Imports an armored public or secret key block (auto-detected) and returns the parsed key.</summary>
    PgpKey Import(string armored);

    /// <summary>Removes a key matched by identity, key id, or fingerprint. Returns whether one was removed.</summary>
    bool Remove(string identityOrKeyIdOrFingerprint);

    /// <summary>Finds a key by identity, key id, or fingerprint; null when absent.</summary>
    PgpKey? Find(string identityOrKeyIdOrFingerprint);

    /// <summary>Whether a key matching identity, key id, or fingerprint is present.</summary>
    bool Contains(string identityOrKeyIdOrFingerprint);

    /// <summary>Replaces the keyring contents with the key store's persisted keys.</summary>
    Task LoadAsync(IPgpKeyStore store, CancellationToken cancellationToken = default);

    /// <summary>Persists the keyring contents to the key store.</summary>
    Task SaveAsync(IPgpKeyStore store, CancellationToken cancellationToken = default);
}
