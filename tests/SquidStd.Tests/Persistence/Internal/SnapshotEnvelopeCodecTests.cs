using SquidStd.Persistence.Abstractions.Data;
using SquidStd.Persistence.Internal;

namespace SquidStd.Tests.Persistence.Internal;

public class SnapshotEnvelopeCodecTests
{
    [Fact]
    public void EncodeDecode_RoundTrips()
    {
        var envelope = new SnapshotFileEnvelope
        {
            Version = 1,
            LastSequenceId = 99,
            Checksum = 123456u,
            Bucket = new EntitySnapshotBucket
            {
                TypeId = 3,
                TypeName = "PlayerData",
                SchemaVersion = 2,
                Payload = [9, 8, 7]
            }
        };

        var decoded = SnapshotEnvelopeCodec.Decode(SnapshotEnvelopeCodec.Encode(envelope));

        Assert.Equal(envelope.Version, decoded.Version);
        Assert.Equal(envelope.LastSequenceId, decoded.LastSequenceId);
        Assert.Equal(envelope.Checksum, decoded.Checksum);
        Assert.Equal(envelope.Bucket.TypeId, decoded.Bucket.TypeId);
        Assert.Equal(envelope.Bucket.TypeName, decoded.Bucket.TypeName);
        Assert.Equal(envelope.Bucket.SchemaVersion, decoded.Bucket.SchemaVersion);
        Assert.Equal(envelope.Bucket.Payload, decoded.Bucket.Payload);
    }
}
