using DryIoc;
using SquidStd.Crypto.Pgp.Interfaces;
using SquidStd.Crypto.Pgp.Services;

namespace SquidStd.Crypto.Pgp.Extensions;

/// <summary>DryIoc registration helpers for the PGP module.</summary>
public static class RegisterPgpExtensions
{
    /// <param name="container">Container that receives the PGP registrations.</param>
    extension(IContainer container)
    {
        /// <summary>
        /// Registers the PGP keyring, service, and the chosen key store (all singletons). The keyring is not
        /// auto-loaded; call <see cref="IPgpKeyring.LoadAsync" /> at startup if persistence is desired.
        /// </summary>
        /// <param name="keyStoreFactory">Builds the <see cref="IPgpKeyStore" /> from the resolver.</param>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterPgp(Func<IResolver, IPgpKeyStore> keyStoreFactory)
        {
            ArgumentNullException.ThrowIfNull(keyStoreFactory);

            container.Register<IPgpKeyring, PgpKeyring>(Reuse.Singleton);
            container.Register<IPgpService, PgpService>(Reuse.Singleton);
            container.RegisterDelegate(keyStoreFactory, Reuse.Singleton);

            return container;
        }
    }
}
