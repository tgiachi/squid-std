using SquidStd.Mail.Abstractions.Data.Config;

namespace SquidStd.Tests.Mail;

public class SmtpOptionsTests
{
    [Fact]
    public void Defaults_AreSensible()
    {
        var options = new SmtpOptions();

        Assert.Equal(587, options.Port);
        Assert.False(options.UseSsl);
        Assert.Equal(string.Empty, options.Host);
        Assert.Null(options.DefaultFrom);
    }
}
