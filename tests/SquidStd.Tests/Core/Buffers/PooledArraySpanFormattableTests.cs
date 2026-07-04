using SquidStd.Core.Buffers;

namespace SquidStd.Tests.Core.Buffers;

public class PooledArraySpanFormattableTests
{
    private static PooledArraySpanFormattable Create(string content)
    {
        var array = STArrayPool<char>.Shared.Rent(content.Length);
        content.CopyTo(array);

        return new(array, content.Length);
    }

    [Fact]
    public void Chars_ExposesTheWrittenSlice()
    {
        using var formattable = Create("abc");

        Assert.Equal("abc", formattable.Chars.ToString());
    }

    [Fact]
    public void ToString_ReturnsContent_AndIsIdempotent()
    {
        var formattable = Create("hello");

        var first = formattable.ToString(null, null);
        var second = formattable.ToString(null, null);

        Assert.Equal("hello", first);
        Assert.Same(first, second);
    }

    [Fact]
    public void ImplicitStringConversion_ReturnsContent()
    {
        var formattable = Create("implicit");

        string value = formattable;

        Assert.Equal("implicit", value);
    }

    [Fact]
    public void TryFormat_CopiesIntoDestination_AndReturnsArray()
    {
        var formattable = Create("copy me");
        Span<char> destination = stackalloc char[16];

        var ok = formattable.TryFormat(destination, out var written);

        Assert.True(ok);
        Assert.Equal(7, written);
        Assert.Equal("copy me", destination[..written].ToString());
    }

    [Fact]
    public void TryFormat_DestinationTooSmall_ReturnsFalse()
    {
        using var formattable = Create("too long for this");
        Span<char> destination = stackalloc char[4];

        var ok = formattable.TryFormat(destination, out var written);

        Assert.False(ok);
        Assert.Equal(0, written);
    }
}
