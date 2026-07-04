using SquidStd.Core.Buffers;
using SquidStd.Core.Extensions.Strings;

namespace SquidStd.Tests.Core.Extensions.Strings;

public class StringHelpersTests
{
    [Theory]
    [InlineData("hello world", "Hello World")]
    [InlineData("the quick fox", "the Quick Fox")]
    [InlineData("a", "A")]
    [InlineData("", "")]
    public void Capitalize_UppercasesWordsSkippingLeadingThe(string input, string expected)
        => Assert.Equal(expected, input.Capitalize());

    [Theory]
    [InlineData("value", "default", "value")]
    [InlineData("", "default", "default")]
    [InlineData("   ", "default", "default")]
    [InlineData(null, "default", "default")]
    public void DefaultIfNullOrEmpty_FallsBackOnBlank(string? input, string fallback, string expected)
        => Assert.Equal(expected, input.DefaultIfNullOrEmpty(fallback));

    [Fact]
    public void IndentMultiline_PrefixesEveryLine()
        => Assert.Equal("\ta\n\tb", "a\nb".IndentMultiline());

    [Fact]
    public void TrimMultiline_TrimsEveryLine()
        => Assert.Equal("a\nb", "  a  \n  b  ".TrimMultiline());

    [Theory]
    [InlineData(new byte[] { 65, 66, 0, 67 }, 1, 2)]
    [InlineData(new byte[] { 65, 66, 67 }, 1, -1)]
    [InlineData(new byte[] { 65, 0, 0, 0, 66, 0 }, 2, 2)]
    public void IndexOfTerminator_FindsNullTerminatorScaledBySize(byte[] buffer, int sizeT, int expected)
        => Assert.Equal(expected, ((ReadOnlySpan<byte>)buffer).IndexOfTerminator(sizeT));

    [Fact]
    public void Remove_WithComparison_RemovesFullMatches()
    {
        Assert.Equal("aa", "aBCabc".AsSpan().Remove("bc".AsSpan(), StringComparison.OrdinalIgnoreCase));
        Assert.Equal("abcabc", "abcabc".AsSpan().Remove("X".AsSpan(), StringComparison.Ordinal));
        Assert.Equal(string.Empty, ReadOnlySpan<char>.Empty.Remove("x".AsSpan(), StringComparison.Ordinal));
    }

    [Fact]
    public void Remove_BufferTooSmall_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            Span<char> buffer = stackalloc char[2];
            "abcdef".AsSpan().Remove("x".AsSpan(), StringComparison.Ordinal, buffer, out _);
        });
    }

    [Fact]
    public void ReplaceAny_SwapsInvalidCharsWithReplacements()
    {
        Span<char> chars = stackalloc char[5];
        "a/b\\c".AsSpan().CopyTo(chars);

        chars.ReplaceAny("/\\".AsSpan(), "--".AsSpan());

        Assert.Equal("a-b-c", chars.ToString());
    }

    [Fact]
    public void ToPooledArray_CopiesContent_CallerReturnsIt()
    {
        var array = "pooled".ToPooledArray();

        Assert.True(array.Length >= 6);
        Assert.Equal("pooled", array.AsSpan(0, 6).ToString());

        STArrayPool<char>.Shared.Return(array);
    }

    [Fact]
    public void Wrap_WrapsAtWordBoundaries()
    {
        var lines = "the quick brown fox jumps".Wrap(11, 5);

        Assert.Equal(["the quick", "brown fox", "jumps"], lines);
    }

    [Fact]
    public void Wrap_BreaksWordsLongerThanLine()
    {
        var lines = "abcdefghij".Wrap(4, 5);

        Assert.Equal(["abcd", "efgh", "ij"], lines);
    }

    [Fact]
    public void Wrap_StopsAtMaxLines()
    {
        var lines = "a b c d e".Wrap(1, 2);

        Assert.Equal(["a", "b"], lines);
    }

    [Fact]
    public void Wrap_EmptyInput_ReturnsEmptyList()
        => Assert.Empty("   ".Wrap(10, 3));

    [Fact]
    public void AppendSpaceWithArticle_PrependsArticleOnlyWhenEmpty()
    {
        var builder = new ValueStringBuilder(stackalloc char[32]);
        builder.AppendSpaceWithArticle("apple", articleAn: true);
        Assert.Equal("an apple", builder.ToString());

        var second = new ValueStringBuilder(stackalloc char[32]);
        second.Append("one");
        second.AppendSpaceWithArticle("sword", articleAn: false);
        Assert.Equal("one sword", second.ToString());
    }
}
