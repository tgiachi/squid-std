using System.Text;
using SquidStd.Crypto.Pgp.Data;
using SquidStd.Crypto.Pgp.Interfaces;
using SquidStd.Crypto.Pgp.Internal;

namespace SquidStd.Crypto.Pgp.Services;

/// <summary>
///     Key store backed by a directory of armored <c>.asc</c> files (one public, optionally one secret, per
///     key). gpg-interoperable.
/// </summary>
public sealed class FilePgpKeyStore : IPgpKeyStore
{
    private const string PublicSuffix = ".pub.asc";
    private const string SecretSuffix = ".sec.asc";
    private readonly string _directory;

    public FilePgpKeyStore(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        _directory = directory;
    }

    /// <inheritdoc />
    public async Task SaveAsync(IReadOnlyCollection<PgpKey> keys, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(keys);
        Directory.CreateDirectory(_directory);

        foreach (var key in keys)
        {
            var stem = Stem(key);
            await File.WriteAllTextAsync(
                    Path.Combine(_directory, stem + PublicSuffix),
                    key.PublicArmored,
                    cancellationToken
                )
                .ConfigureAwait(false);

            if (key.PrivateArmored is not null)
            {
                await File.WriteAllTextAsync(
                        Path.Combine(_directory, stem + SecretSuffix),
                        key.PrivateArmored,
                        cancellationToken
                    )
                    .ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PgpKey>> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_directory))
        {
            return [];
        }

        var result = new List<PgpKey>();

        foreach (var publicPath in Directory.EnumerateFiles(_directory, "*" + PublicSuffix))
        {
            var stem = Path.GetFileName(publicPath)[..^PublicSuffix.Length];
            var publicArmored = await File.ReadAllTextAsync(publicPath, cancellationToken).ConfigureAwait(false);

            var secretPath = Path.Combine(_directory, stem + SecretSuffix);
            string? secretArmored = File.Exists(secretPath)
                ? await File.ReadAllTextAsync(secretPath, cancellationToken).ConfigureAwait(false)
                : null;

            result.Add(PgpKeyFactory.FromArmored(publicArmored, secretArmored));
        }

        return result;
    }

    private static string Stem(PgpKey key)
    {
        var safeIdentity = new StringBuilder(key.Identity.Length);
        foreach (var ch in key.Identity)
        {
            safeIdentity.Append(char.IsLetterOrDigit(ch) ? ch : '_');
        }

        return safeIdentity + "." + key.KeyId;
    }
}
