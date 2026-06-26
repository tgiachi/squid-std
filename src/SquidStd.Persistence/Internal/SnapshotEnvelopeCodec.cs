using System.Buffers.Binary;
using System.Text;
using SquidStd.Persistence.Abstractions.Data;

namespace SquidStd.Persistence.Internal;

/// <summary>
/// Fixed-layout binary encoder/decoder for a <see cref="SnapshotFileEnvelope" />.
/// Layout: int Version | long LastSeq | uint Checksum | ushort TypeId | int NameLen | name UTF8 |
/// int SchemaVersion | int PayloadLen | payload bytes.
/// </summary>
internal static class SnapshotEnvelopeCodec
{
    public static byte[] Encode(SnapshotFileEnvelope envelope)
    {
        var nameBytes = Encoding.UTF8.GetBytes(envelope.Bucket.TypeName);
        var payload = envelope.Bucket.Payload;
        var buffer = new byte[4 + 8 + 4 + 2 + 4 + nameBytes.Length + 4 + 4 + payload.Length];
        var span = buffer.AsSpan();
        var offset = 0;

        BinaryPrimitives.WriteInt32LittleEndian(span[offset..], envelope.Version);
        offset += 4;
        BinaryPrimitives.WriteInt64LittleEndian(span[offset..], envelope.LastSequenceId);
        offset += 8;
        BinaryPrimitives.WriteUInt32LittleEndian(span[offset..], envelope.Checksum);
        offset += 4;
        BinaryPrimitives.WriteUInt16LittleEndian(span[offset..], envelope.Bucket.TypeId);
        offset += 2;
        BinaryPrimitives.WriteInt32LittleEndian(span[offset..], nameBytes.Length);
        offset += 4;
        nameBytes.CopyTo(span[offset..]);
        offset += nameBytes.Length;
        BinaryPrimitives.WriteInt32LittleEndian(span[offset..], envelope.Bucket.SchemaVersion);
        offset += 4;
        BinaryPrimitives.WriteInt32LittleEndian(span[offset..], payload.Length);
        offset += 4;
        payload.CopyTo(span[offset..]);

        return buffer;
    }

    public static SnapshotFileEnvelope Decode(ReadOnlySpan<byte> bytes)
    {
        var offset = 0;
        var version = BinaryPrimitives.ReadInt32LittleEndian(bytes[offset..]);
        offset += 4;
        var lastSequenceId = BinaryPrimitives.ReadInt64LittleEndian(bytes[offset..]);
        offset += 8;
        var checksum = BinaryPrimitives.ReadUInt32LittleEndian(bytes[offset..]);
        offset += 4;
        var typeId = BinaryPrimitives.ReadUInt16LittleEndian(bytes[offset..]);
        offset += 2;
        var nameLength = BinaryPrimitives.ReadInt32LittleEndian(bytes[offset..]);
        offset += 4;
        var typeName = Encoding.UTF8.GetString(bytes.Slice(offset, nameLength));
        offset += nameLength;
        var schemaVersion = BinaryPrimitives.ReadInt32LittleEndian(bytes[offset..]);
        offset += 4;
        var payloadLength = BinaryPrimitives.ReadInt32LittleEndian(bytes[offset..]);
        offset += 4;
        var payload = bytes.Slice(offset, payloadLength).ToArray();

        return new SnapshotFileEnvelope
        {
            Version = version,
            LastSequenceId = lastSequenceId,
            Checksum = checksum,
            Bucket = new EntitySnapshotBucket
            {
                TypeId = typeId,
                TypeName = typeName,
                SchemaVersion = schemaVersion,
                Payload = payload
            }
        };
    }
}
