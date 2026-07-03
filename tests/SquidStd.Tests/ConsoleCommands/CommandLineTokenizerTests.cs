using SquidStd.ConsoleCommands.Internal;

namespace SquidStd.Tests.ConsoleCommands;

public class CommandLineTokenizerTests
{
    [Theory]
    [InlineData("gc", new[] { "gc" })]
    [InlineData("deploy fast now", new[] { "deploy", "fast", "now" })]
    [InlineData("say \"hello world\" loud", new[] { "say", "hello world", "loud" })]
    [InlineData("say \"unterminated", new[] { "say", "unterminated" })]
    [InlineData("  spaced   out  ", new[] { "spaced", "out" })]
    [InlineData("empty \"\" arg", new[] { "empty", "", "arg" })]
    public void Tokenize_SplitsRespectingQuotes(string input, string[] expected)
        => Assert.Equal(expected, CommandLineTokenizer.Tokenize(input));

    [Fact]
    public void Tokenize_EmptyInput_ReturnsEmpty()
        => Assert.Empty(CommandLineTokenizer.Tokenize("   "));
}
