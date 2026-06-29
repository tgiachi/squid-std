using SquidStd.Crypto.Pgp.Data;

namespace SquidStd.Crypto.Pgp.Interfaces;

/// <summary>
///     Persistence backend for a set of PGP keys. Implementations decide the on-disk representation.
/// </summary>
public interface IPgpKeyStore
{
    /// <summary>Persists the supplied keys, replacing any previously stored set.</summary>
    Task SaveAsync(IReadOnlyCollection<PgpKey> keys, CancellationToken cancellationToken = default);

    /// <summary>Loads all persisted keys. Returns an empty list when nothing is stored.</summary>
    Task<IReadOnlyList<PgpKey>> LoadAsync(CancellationToken cancellationToken = default);
}
