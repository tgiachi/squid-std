using SquidStd.Mail.Queue.Data.Config;

namespace SquidStd.Tests.Mail;

public class MailQueueOptionsTests
{
    [Fact]
    public void Defaults_AreSensible()
    {
        var options = new MailQueueOptions();

        Assert.Equal("squidstd.mail.outbound", options.QueueName);
    }
}
