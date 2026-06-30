using DryIoc;
using SquidStd.Database.Abstractions.Interfaces.Data;
using SquidStd.Vfs.Abstractions.Interfaces;
using SquidStd.Vfs.Database.Data.Entities;
using SquidStd.Vfs.Database.Services;

namespace SquidStd.Vfs.Database.Extensions;

/// <summary>DryIoc registration helper for the database VFS backend.</summary>
public static class RegisterDatabaseFileSystemExtensions
{
    /// <param name="container">Container that receives the VFS registration.</param>
    extension(IContainer container)
    {
        /// <summary>
        /// Registers an <see cref="IVirtualFileSystem" /> backed by the database.
        /// Requires the SquidStd.Database module to be registered (it supplies <c>IDataAccess&lt;&gt;</c>).
        /// </summary>
        public IContainer RegisterDatabaseFileSystem()
        {
            container.RegisterDelegate<IVirtualFileSystem>(
                r => new DatabaseFileSystem(r.Resolve<IDataAccess<VfsFileEntity>>()),
                Reuse.Singleton
            );

            return container;
        }
    }
}
