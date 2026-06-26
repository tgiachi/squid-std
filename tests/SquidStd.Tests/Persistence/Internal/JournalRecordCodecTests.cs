using SquidStd.Persistence.Abstractions.Data;
using SquidStd.Persistence.Abstractions.Types;
using SquidStd.Persistence.Internal;

namespace SquidStd.Tests.Persistence.Internal;

public class JournalRecordCodecTests
{
    [Fact]
    public void EncodeDecode_RoundTrips()
    {
        var entry = new JournalEntry
        {
            SequenceId = 42,
            TimestampUnixMilliseconds = 1_700_000_000_000,
            TypeId = 7,
            Operation = JournalEntityOperationType.Upsert,
            Payload = [1, 2, 3, 4, 5]
        };

        var bytes = JournalRecordCodec.Encode(entry);
        var decoded = JournalRecordCodec.Decode(bytes);

        Assert.Equal(entry.SequenceId, decoded.SequenceId);
        Assert.Equal(entry.TimestampUnixMilliseconds, decoded.TimestampUnixMilliseconds);
        Assert.Equal(entry.TypeId, decoded.TypeId);
        Assert.Equal(entry.Operation, decoded.Operation);
        Assert.Equal(entry.Payload, decoded.Payload);
    }

    [Fact]
    public void EncodeDecode_EmptyPayload_RoundTrips()
    {
        var entry = new JournalEntry { SequenceId = 1, TypeId = 1, Operation = JournalEntityOperationType.Remove };

        var decoded = JournalRecordCodec.Decode(JournalRecordCodec.Encode(entry));

        Assert.Empty(decoded.Payload);
        Assert.Equal(JournalEntityOperationType.Remove, decoded.Operation);
    }
}
