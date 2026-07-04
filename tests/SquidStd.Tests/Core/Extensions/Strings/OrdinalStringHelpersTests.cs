using SquidStd.Core.Extensions.Strings;

namespace SquidStd.Tests.Core.Extensions.Strings;

public class OrdinalStringHelpersTests
{
    [Theory]
    [InlineData("hello world", "hello", true)]
    [InlineData("hello world", "Hello", false)]
    [InlineData("hello", "hello world", false)]
    public void StartsWithOrdinal_String(string a, string b, bool expected)
        => Assert.Equal(expected, a.StartsWithOrdinal(b));

    [Fact]
    public void StartsWithOrdinal_NullInstance_ReturnsFalse()
        => Assert.False(((string?)null).StartsWithOrdinal("x"));

    [Fact]
    public void StartsWithOrdinal_Span()
    {
        Assert.True("hello".AsSpan().StartsWithOrdinal("he".AsSpan()));
        Assert.True("hello".AsSpan().StartsWithOrdinal('h'));
        Assert.False("hello".AsSpan().StartsWithOrdinal('H'));
        Assert.False(ReadOnlySpan<char>.Empty.StartsWithOrdinal('h'));
    }

    [Theory]
    [InlineData("hello world", "world", true)]
    [InlineData("hello world", "World", false)]
    public void EndsWithOrdinal_String(string a, string b, bool expected)
        => Assert.Equal(expected, a.EndsWithOrdinal(b));

    [Fact]
    public void EndsWithOrdinal_Span()
    {
        Assert.True("hello".AsSpan().EndsWithOrdinal("lo".AsSpan()));
        Assert.True("hello".AsSpan().EndsWithOrdinal('o'));
        Assert.False(ReadOnlySpan<char>.Empty.EndsWithOrdinal('o'));
    }

    [Fact]
    public void EqualsOrdinal_IsCaseSensitive_AndNullSafe()
    {
        Assert.True("abc".EqualsOrdinal("abc"));
        Assert.False("abc".EqualsOrdinal("ABC"));
        Assert.True(((string?)null).EqualsOrdinal(null));
        Assert.False("abc".EqualsOrdinal(null));
        Assert.True("abc".AsSpan().EqualsOrdinal("abc"));
    }

    [Fact]
    public void ContainsOrdinal_StringSpanAndChar()
    {
        Assert.True("hello world".ContainsOrdinal("lo w"));
        Assert.False("hello world".ContainsOrdinal("LO W"));
        Assert.True("hello".ContainsOrdinal('e'));
        Assert.True("hello".AsSpan().ContainsOrdinal("ell"));
        Assert.True("hello".AsSpan().ContainsOrdinal("ell".AsSpan()));
    }

    [Fact]
    public void CompareOrdinal_MatchesStringCompareOrdinal()
    {
        Assert.Equal(string.CompareOrdinal("a", "b"), "a".CompareOrdinal("b"));
        Assert.Equal(0, "abc".AsSpan().CompareOrdinal("abc".AsSpan()));
    }

    [Fact]
    public void IndexOfOrdinal_AllOverloads()
    {
        Assert.Equal(1, "hello".IndexOfOrdinal('e'));
        Assert.Equal(2, "ababa".IndexOfOrdinal("ab", 1));
        Assert.Equal(-1, "hello".IndexOfOrdinal("L"));
        Assert.Equal(1, "hello".AsSpan().IndexOfOrdinal('e'));
        Assert.Equal(2, "hello".AsSpan().IndexOfOrdinal("ll".AsSpan()));
    }

    [Fact]
    public void RemoveOrdinal_RemovesAllMatches()
    {
        Assert.Equal("aa", "abcabc".RemoveOrdinal("bc"));
        Assert.Equal("aa", "abcabc".AsSpan().RemoveOrdinal("bc".AsSpan()));

        Span<char> buffer = stackalloc char[8];
        "abcabc".AsSpan().RemoveOrdinal("bc".AsSpan(), buffer, out var size);
        Assert.Equal("aa", buffer[..size].ToString());
    }

    [Fact]
    public void ReplaceOrdinal_IsCaseSensitive()
    {
        Assert.Equal("aXcaXc", "abcabc".ReplaceOrdinal("b", "X"));
        Assert.Equal("abcabc", "abcabc".ReplaceOrdinal("B", "X"));
    }
}
