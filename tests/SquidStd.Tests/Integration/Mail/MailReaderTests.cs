using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SquidStd.Mail.Abstractions.Data.Config;
using SquidStd.Mail.Abstractions.Types.Mail;
using SquidStd.Mail.MailKit.Services;

namespace SquidStd.Tests.Integration.Mail;

[Collection(GreenMailCollection.Name)]
public class MailReaderTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);

    private readonly GreenMailContainerFixture _fixture;

    public MailReaderTests(GreenMailContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Imap_FetchesNewMessage_AndMarksSeen()
    {
        var user = "imap-user@example.com";
        await SendAsync(user, "imap-subject", true);
        var reader = new ImapMailReader(ImapOptions(user));

        var first = await reader.FetchNewAsync().WaitAsync(Timeout);
        var second = await reader.FetchNewAsync().WaitAsync(Timeout);

        var message = Assert.Single(first);
        Assert.Equal("imap-subject", message.Subject);
        Assert.Single(message.Attachments);
        Assert.NotNull(message.RawEml);
        Assert.Empty(second);
    }

    [Fact]
    public async Task Pop3_FetchesNewMessage_WithDelete()
    {
        var user = "pop-user@example.com";
        await SendAsync(user, "pop-subject", false);
        var reader = new Pop3MailReader(
            new MailOptions
            {
                Protocol = MailProtocolType.Pop3,
                Host = _fixture.Host,
                Port = _fixture.Pop3Port,
                UseSsl = false,
                Username = user,
                Password = "pwd",
                DeleteAfterDownload = true
            }
        );

        var first = await reader.FetchNewAsync().WaitAsync(Timeout);
        var second = await reader.FetchNewAsync().WaitAsync(Timeout);

        Assert.Contains(first, m => m.Subject == "pop-subject");
        Assert.Empty(second);
    }

    private MailOptions ImapOptions(string user)
    {
        return new MailOptions
        {
            Protocol = MailProtocolType.Imap,
            Host = _fixture.Host,
            Port = _fixture.ImapPort,
            UseSsl = false,
            Username = user,
            Password = "pwd",
            MarkAsSeen = true,
            IncludeRawEml = true
        };
    }

    private async Task SendAsync(string to, string subject, bool withAttachment)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Sender", "sender@example.com"));
        message.To.Add(new MailboxAddress("Rcpt", to));
        message.Subject = subject;

        var builder = new BodyBuilder { TextBody = "body" };

        if (withAttachment)
        {
            builder.Attachments.Add("a.txt", Encoding.UTF8.GetBytes("xyz"), ContentType.Parse("text/plain"));
        }

        message.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_fixture.Host, _fixture.SmtpPort, SecureSocketOptions.None).WaitAsync(Timeout);
        await smtp.SendAsync(message).WaitAsync(Timeout);
        await smtp.DisconnectAsync(true).WaitAsync(Timeout);
    }
}
