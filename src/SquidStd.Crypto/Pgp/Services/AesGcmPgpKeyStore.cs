using SquidStd.Core.Interfaces.Secrets;
using SquidStd.Crypto.Pgp.Data;
using SquidStd.Crypto.Pgp.Interfaces;
using SquidStd.Crypto.Pgp.Internal;

namespace SquidStd.Crypto.Pgp.Services;

/// <summary>
/// Key store that serializes the keyring to a single file encrypted at rest with the application key via
/// <see cref="ISecretProtector" />.
/// </summary>
public sealed class AesGcmPgpKeyStore : IPgpKeyStore
{
    private readonly ISecretProtector _protector;
    private readonly string _path;

    public AesGcmPgpKeyStore(ISecretProtector protector, string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _protector = protector;
        _path = path;
    }

    /// <inheritdoc />
    public async Task SaveAsync(IReadOnlyCollection<PgpKey> keys, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(keys);

        var plaintext = PgpKeyStoreCodec.Encode(keys);
        var protectedBytes = _protector.Protect(plaintext);

        var directory = Path.GetDirectoryName(_path);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllBytesAsync(_path, protectedBytes, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PgpKey>> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_path))
        {
            return [];
        }

        var protectedBytes = await File.ReadAllBytesAsync(_path, cancellationToken).ConfigureAwait(false);
        var plaintext = _protector.Unprotect(protectedBytes);

        return PgpKeyStoreCodec.Decode(plaintext);
    }
}
