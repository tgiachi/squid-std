namespace SquidStd.Messaging.Abstractions.Interfaces;

/// <summary>
/// Serializes and deserializes queue message payloads.
/// </summary>
public interface IMessageSerializer
{
    /// <summary>Serializes a message to bytes.</summary>
    ReadOnlyMemory<byte> Serialize<TMessage>(TMessage message);

    /// <summary>Deserializes bytes to a message.</summary>
    TMessage Deserialize<TMessage>(ReadOnlyMemory<byte> payload);
}
