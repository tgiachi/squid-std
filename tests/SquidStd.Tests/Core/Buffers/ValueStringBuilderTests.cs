using SquidStd.Core.Buffers;

namespace SquidStd.Tests.Core.Buffers;

public class ValueStringBuilderTests
{
    [Fact]
    public void Append_CharsAndStrings_BuildsContent()
    {
        using var builder = new ValueStringBuilder(stackalloc char[16]);

        builder.Append('a');
        builder.Append("bc");
        builder.Append('d', 3);

        Assert.Equal(6, builder.Length);
        Assert.Equal("abcddd", builder.ToString());
    }

    [Fact]
    public void Append_GrowsBeyondInitialBuffer_PreservesContent()
    {
        var builder = new ValueStringBuilder(stackalloc char[4]);
        var expected = new string('x', 100);

        builder.Append(expected);

        Assert.Equal(100, builder.Length);
        Assert.Equal(expected, builder.ToString());
    }

    [Fact]
    public void Constructor_FromInitialString_CopiesIt()
    {
        var builder = new ValueStringBuilder("hello");

        Assert.Equal("hello", builder.ToString());
    }

    [Fact]
    public void Insert_ShiftsExistingContent()
    {
        var builder = new ValueStringBuilder(stackalloc char[16]);
        builder.Append("held");

        builder.Insert(2, "llo wor");

        Assert.Equal("hello world", builder.ToString());
    }

    [Fact]
    public void Indexer_AllowsInPlaceMutation()
    {
        var builder = new ValueStringBuilder(stackalloc char[8]);
        builder.Append("cat");

        builder[0] = 'b';

        Assert.Equal("bat", builder.ToString());
    }

    [Fact]
    public void EnsureCapacity_GrowsCapacity()
    {
        var builder = new ValueStringBuilder(4);

        builder.EnsureCapacity(64);

        Assert.True(builder.Capacity >= 64);
        builder.Dispose();
    }

    [Fact]
    public void Append_InterpolatedHandler_FormatsValues()
    {
        var builder = new ValueStringBuilder(stackalloc char[32]);

        builder.Append($"value {42} and {"text"}");

        Assert.Equal("value 42 and text", builder.ToString());
    }

    [Fact]
    public void MultiThreadedMode_UsesSharedPoolAndRoundTrips()
    {
        var builder = new ValueStringBuilder(8, mt: true);

        builder.Append("thread safe pool path");

        Assert.Equal("thread safe pool path", builder.ToString());
    }
}
