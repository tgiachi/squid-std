using DryIoc;
using SquidStd.Vfs.Abstractions.Interfaces;
using SquidStd.Vfs.S3.Data;
using SquidStd.Vfs.S3.Services;

namespace SquidStd.Vfs.S3.Extensions;

/// <summary>DryIoc registration helper for the S3-compatible VFS backend.</summary>
public static class RegisterS3FileSystemExtensions
{
    /// <param name="container">Container that receives the VFS registration.</param>
    extension(IContainer container)
    {
        /// <summary>Registers an <see cref="IVirtualFileSystem" /> backed by S3-compatible storage.</summary>
        public IContainer RegisterS3FileSystem(Action<S3FileSystemOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(configure);

            var options = new S3FileSystemOptions();
            configure(options);
            container.RegisterDelegate<IVirtualFileSystem>(_ => new S3FileSystem(options), Reuse.Singleton);

            return container;
        }
    }
}
