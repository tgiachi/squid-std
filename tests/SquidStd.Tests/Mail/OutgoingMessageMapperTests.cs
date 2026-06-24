using System.Text;
using MimeKit;
using SquidStd.Mail.Abstractions.Data;
using SquidStd.Mail.Abstractions.Data.Config;
using SquidStd.Mail.MailKit.Services;

namespace SquidStd.Tests.Mail;

public class OutgoingMessageMapperTests
{
    [Fact]
    public void ToMimeMessage_FallsBackToDefaultFrom()
    {
        var options = new SmtpOptions { DefaultFrom = new("Sys", "sys@example.com") };

        var mime = OutgoingMessageMapper.ToMimeMessage(Message(), options);

        Assert.Equal("sys@example.com", mime.From.Mailboxes.Single().Address);
    }

    [Fact]
    public void ToMimeMessage_MapsRecipientsSubjectBodiesAndAttachment()
    {
        var options = new SmtpOptions { DefaultFrom = new("Sys", "sys@example.com") };

        var mime = OutgoingMessageMapper.ToMimeMessage(Message(new("Alice", "alice@example.com")), options);

        Assert.Equal("alice@example.com", mime.From.Mailboxes.Single().Address);
        Assert.Contains(mime.To.Mailboxes, m => m.Address == "bob@example.com");
        Assert.Contains(mime.Cc.Mailboxes, m => m.Address == "carol@example.com");
        Assert.Contains(mime.Bcc.Mailboxes, m => m.Address == "dave@example.com");
        Assert.Equal("Hello", mime.Subject);
        Assert.Equal("plain", mime.TextBody);
        Assert.Equal("<p>html</p>", mime.HtmlBody);
        Assert.Contains(mime.Attachments.OfType<MimePart>(), p => p.FileName == "a.txt");
    }

    [Fact]
    public void ToMimeMessage_Throws_WhenNoFromAndNoDefault()
        => Assert.Throws<ArgumentException>(() => OutgoingMessageMapper.ToMimeMessage(Message(), new()));

    private static OutgoingMailMessage Message(MailAddress? from = null)
        => new()
        {
            From = from,
            To = [new("Bob", "bob@example.com")],
            Cc = [new("Carol", "carol@example.com")],
            Bcc = [new("Dave", "dave@example.com")],
            Subject = "Hello",
            TextBody = "plain",
            HtmlBody = "<p>html</p>",
            Attachments = [new("a.txt", "text/plain", Encoding.UTF8.GetBytes("xyz"))]
        };
}
