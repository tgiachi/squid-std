using DryIoc;
using SquidStd.Core.Interfaces.Secrets;
using SquidStd.Secrets.Aws.Data;
using SquidStd.Secrets.Aws.Services;

namespace SquidStd.Secrets.Aws.Extensions;

/// <summary>DryIoc registration helper for the KMS secret protector.</summary>
public static class RegisterKmsSecretProtectorExtensions
{
    /// <param name="container">Container that receives the protector registration.</param>
    extension(IContainer container)
    {
        /// <summary>Registers <see cref="KmsSecretProtector" /> as the singleton <see cref="ISecretProtector" />.</summary>
        public IContainer RegisterKmsSecretProtector(Action<KmsSecretProtectorOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(configure);

            var options = new KmsSecretProtectorOptions();
            configure(options);
            container.RegisterInstance<ISecretProtector>(new KmsSecretProtector(options));

            return container;
        }
    }
}
