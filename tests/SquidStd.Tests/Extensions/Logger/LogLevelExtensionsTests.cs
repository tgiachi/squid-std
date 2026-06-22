using Serilog.Events;
using SquidStd.Core.Extensions.Logger;
using SquidStd.Core.Types;

namespace SquidStd.Tests.Extensions.Logger;

public class LogLevelExtensionsTests
{
    [Theory]
    [InlineData(LogLevelType.Trace, LogEventLevel.Verbose)]
    [InlineData(LogLevelType.Debug, LogEventLevel.Debug)]
    [InlineData(LogLevelType.Information, LogEventLevel.Information)]
    [InlineData(LogLevelType.Warning, LogEventLevel.Warning)]
    [InlineData(LogLevelType.Error, LogEventLevel.Error)]
    public void ToSerilogLogLevel_KnownLevels_MapsExpected(LogLevelType input, LogEventLevel expected)
        => Assert.Equal(expected, input.ToSerilogLogLevel());

    [Theory]
    [InlineData(LogLevelType.None)]
    [InlineData(LogLevelType.Critical)]
    public void ToSerilogLogLevel_UnmappedLevels_FallsBackToInformation(LogLevelType input)
        => Assert.Equal(LogEventLevel.Information, input.ToSerilogLogLevel());
}
