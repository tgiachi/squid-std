using DryIoc;
using SquidStd.Abstractions.Extensions.Config;
using SquidStd.Storage.Abstractions.Data.Config;
using SquidStd.Storage.Abstractions.Interfaces;
using SquidStd.Storage.Services;

namespace SquidStd.Storage.Extensions;

/// <summary>
/// DryIoc registration helpers for the local file storage provider.
/// </summary>
public static class StorageRegistrationExtensions
{
    extension(IContainer container)
    {
        /// <summary>Registers file-backed <see cref="IStorageService" /> and YAML-backed <see cref="IObjectStorageService" />.</summary>
        public IContainer AddFileStorage(StorageConfig? config = null)
        {
            ArgumentNullException.ThrowIfNull(container);

            if (config is not null)
            {
                container.RegisterInstance(config, IfAlreadyRegistered.Replace);
            }
            else
            {
                container.RegisterConfigSection("storage", static () => new StorageConfig(), -70);
                container.RegisterInstance(new StorageConfig(), IfAlreadyRegistered.Keep);
            }

            container.Register<IStorageService, FileStorageService>(Reuse.Singleton);
            container.Register<IObjectStorageService, YamlObjectStorageService>(Reuse.Singleton);

            return container;
        }
    }
}
