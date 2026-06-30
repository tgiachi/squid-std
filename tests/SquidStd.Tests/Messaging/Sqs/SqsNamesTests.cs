using SquidStd.Messaging.Sqs.Internal;

namespace SquidStd.Tests.Messaging.Sqs;

public class SqsNamesTests
{
    [Fact]
    public void Sanitize_ReplacesDotsWithDashes()
        => Assert.Equal("orders-dlq", SqsNames.Sanitize("orders.dlq"));

    [Fact]
    public void Sanitize_KeepsLettersDigitsDashUnderscore()
        => Assert.Equal("a_b-9", SqsNames.Sanitize("a_b-9"));

    [Fact]
    public void Sanitize_ReplacesSlashesAndColons()
        => Assert.Equal("a-b-c", SqsNames.Sanitize("a/b:c"));
}
