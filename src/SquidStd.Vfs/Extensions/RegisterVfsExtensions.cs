using DryIoc;
using SquidStd.Vfs.Abstractions.Interfaces;

namespace SquidStd.Vfs.Extensions;

/// <summary>DryIoc registration helpers for the VFS module.</summary>
public static class RegisterVfsExtensions
{
    /// <param name="container">Container that receives the VFS registration.</param>
    extension(IContainer container)
    {
        /// <summary>Registers an <see cref="IVirtualFileSystem" /> singleton built by the factory.</summary>
        public IContainer RegisterVfs(Func<IResolver, IVirtualFileSystem> fileSystemFactory)
        {
            ArgumentNullException.ThrowIfNull(fileSystemFactory);
            container.RegisterDelegate(fileSystemFactory, Reuse.Singleton);

            return container;
        }
    }
}
