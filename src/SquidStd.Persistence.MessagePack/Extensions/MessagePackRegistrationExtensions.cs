using DryIoc;
using SquidStd.Core.Interfaces.Serialization;

namespace SquidStd.Persistence.MessagePack.Extensions;

/// <summary>
/// DryIoc registration helpers for the MessagePack data serializer.
/// </summary>
public static class MessagePackRegistrationExtensions
{
    /// <param name="container">Container that receives the registrations.</param>
    extension(IContainer container)
    {
        /// <summary>
        /// Registers the MessagePack serializer for <see cref="IDataSerializer" /> and
        /// <see cref="IDataDeserializer" /> (same singleton instance). Existing registrations
        /// are kept.
        /// </summary>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterMessagePackSerializer()
        {
            var serializer = new MessagePackDataSerializer();
            container.RegisterInstance<IDataSerializer>(serializer, IfAlreadyRegistered.Keep);
            container.RegisterInstance<IDataDeserializer>(serializer, IfAlreadyRegistered.Keep);

            return container;
        }
    }
}
