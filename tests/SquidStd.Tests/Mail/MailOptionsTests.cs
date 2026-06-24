using SquidStd.Mail.Abstractions.Data.Config;
using SquidStd.Mail.Abstractions.Types.Mail;

namespace SquidStd.Tests.Mail;

public class MailOptionsTests
{
    [Fact]
    public void Defaults_AreSensible()
    {
        var options = new MailOptions();

        Assert.Equal(MailProtocolType.Imap, options.Protocol);
        Assert.Equal("INBOX", options.Folder);
        Assert.Equal(60, options.PollIntervalSeconds);
        Assert.True(options.MarkAsSeen);
        Assert.False(options.DeleteAfterDownload);
        Assert.True(options.IncludeAttachmentContent);
        Assert.False(options.IncludeRawEml);
        Assert.Equal(50, options.MaxMessagesPerPoll);
    }
}
