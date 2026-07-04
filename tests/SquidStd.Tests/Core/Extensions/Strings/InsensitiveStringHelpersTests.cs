using SquidStd.Core.Extensions.Strings;

namespace SquidStd.Tests.Core.Extensions.Strings;

public class InsensitiveStringHelpersTests
{
    [Fact]
    public void InsensitiveEquals_IgnoresCase()
    {
        Assert.True("Hello".InsensitiveEquals("hELLO"));
        Assert.False("Hello".InsensitiveEquals("world"));
        Assert.True("Hello".AsSpan().InsensitiveEquals("hELLO".AsSpan()));
        Assert.True("Hello".InsensitiveEquals("hELLO".AsSpan()));
    }

    [Fact]
    public void InsensitiveStartsWithAndEndsWith()
    {
        Assert.True("Hello World".InsensitiveStartsWith("hello"));
        Assert.True("Hello World".AsSpan().InsensitiveStartsWith("HELLO".AsSpan()));
        Assert.True("Hello World".InsensitiveEndsWith("WORLD"));
        Assert.True("Hello World".AsSpan().InsensitiveEndsWith("world".AsSpan()));
        Assert.False(((string?)null).InsensitiveStartsWith("x"));
    }

    [Fact]
    public void InsensitiveContains_AllOverloads()
    {
        Assert.True("Hello World".InsensitiveContains("LO WO"));
        Assert.True("Hello".InsensitiveContains('H'));
        Assert.True("hello".InsensitiveContains('H'));
        Assert.True("Hello".AsSpan().InsensitiveContains("ELL"));
        Assert.True("Hello".AsSpan().InsensitiveContains("ell".AsSpan()));
    }

    [Fact]
    public void InsensitiveCompare_OrdersIgnoringCase()
    {
        Assert.Equal(0, "ABC".InsensitiveCompare("abc"));
        Assert.Equal(0, "ABC".AsSpan().InsensitiveCompare("abc".AsSpan()));
        Assert.True("abc".InsensitiveCompare("ABD") < 0);
    }

    [Fact]
    public void InsensitiveIndexOf_AllOverloads()
    {
        Assert.Equal(0, "Hello".InsensitiveIndexOf('h'));
        Assert.Equal(2, "ababa".InsensitiveIndexOf("AB", 1));
        Assert.Equal(1, "Hello".InsensitiveIndexOf("ELL"));
        Assert.Equal(1, "Hello".AsSpan().InsensitiveIndexOf("ELL".AsSpan()));
    }

    [Fact]
    public void InsensitiveRemove_RemovesAllMatchesIgnoringCase()
    {
        Assert.Equal("aa", "aBCabc".InsensitiveRemove("bc"));
        Assert.Equal("aa", "aBCabc".AsSpan().InsensitiveRemove("bc".AsSpan()));

        Span<char> buffer = stackalloc char[8];
        "aBCabc".AsSpan().InsensitiveRemove("bc".AsSpan(), buffer, out var size);
        Assert.Equal("aa", buffer[..size].ToString());
    }

    [Fact]
    public void InsensitiveReplace_ReplacesIgnoringCase()
        => Assert.Equal("aXaX", "aBcabC".InsensitiveReplace("bc", "X"));
}
