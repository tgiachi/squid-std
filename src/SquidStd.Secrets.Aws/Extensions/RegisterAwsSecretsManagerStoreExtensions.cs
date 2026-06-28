using DryIoc;
using SquidStd.Core.Interfaces.Secrets;
using SquidStd.Secrets.Aws.Data;
using SquidStd.Secrets.Aws.Services;

namespace SquidStd.Secrets.Aws.Extensions;

/// <summary>DryIoc registration helper for the AWS Secrets Manager store.</summary>
public static class RegisterAwsSecretsManagerStoreExtensions
{
    /// <param name="container">Container that receives the store registration.</param>
    extension(IContainer container)
    {
        /// <summary>Registers <see cref="AwsSecretsManagerStore" /> as the singleton <see cref="ISecretStore" />.</summary>
        public IContainer RegisterAwsSecretsManagerStore(Action<AwsSecretsManagerOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(configure);

            var options = new AwsSecretsManagerOptions();
            configure(options);
            container.RegisterInstance<ISecretStore>(new AwsSecretsManagerStore(options));

            return container;
        }
    }
}
