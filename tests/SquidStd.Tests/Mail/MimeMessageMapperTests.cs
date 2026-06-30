using System.Text;
using MimeKit;
using SquidStd.Mail.MailKit.Services;

namespace SquidStd.Tests.Mail;

public class MimeMessageMapperTests
{
    [Fact]
    public void Map_ExtractsHeadersBodiesAndAttachmentContent()
    {
        var result = MimeMessageMapper.Map(BuildMessage(), true, false);

        Assert.Equal("alice@example.com", result.From.Address);
        Assert.Equal("Alice", result.From.Name);
        Assert.Contains(result.To, a => a.Address == "bob@example.com");
        Assert.Contains(result.Cc, a => a.Address == "carol@example.com");
        Assert.Equal("Hello", result.Subject);
        Assert.Equal(DateTimeKind.Utc, result.DateUtc.Kind);
        Assert.Equal("plain text", result.TextBody);
        Assert.Equal("<p>html</p>", result.HtmlBody);

        var attachment = Assert.Single(result.Attachments);
        Assert.Equal("note.txt", attachment.FileName);
        Assert.NotNull(attachment.Content);
        Assert.Equal("file-bytes", Encoding.UTF8.GetString(attachment.Content!));
        Assert.Null(result.RawEml);
    }

    [Fact]
    public void Map_IncludesRawEml_WhenEnabled()
    {
        var result = MimeMessageMapper.Map(BuildMessage(), false, true);

        Assert.NotNull(result.RawEml);
        using var stream = new MemoryStream(result.RawEml!);
        var reparsed = MimeMessage.Load(stream);
        Assert.Equal("Hello", reparsed.Subject);
    }

    [Fact]
    public void Map_OmitsAttachmentContent_WhenDisabled()
    {
        var result = MimeMessageMapper.Map(BuildMessage(), false, false);

        var attachment = Assert.Single(result.Attachments);
        Assert.Null(attachment.Content);
        Assert.True(attachment.Size > 0);
    }

    private static MimeMessage BuildMessage()
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Alice", "alice@example.com"));
        message.To.Add(new MailboxAddress("Bob", "bob@example.com"));
        message.Cc.Add(new MailboxAddress("Carol", "carol@example.com"));
        message.Subject = "Hello";
        message.Date = new(2026, 6, 24, 10, 0, 0, TimeSpan.Zero);
        message.MessageId = "msg-1@example.com";

        var builder = new BodyBuilder
        {
            TextBody = "plain text",
            HtmlBody = "<p>html</p>"
        };
        builder.Attachments.Add("note.txt", Encoding.UTF8.GetBytes("file-bytes"), ContentType.Parse("text/plain"));
        message.Body = builder.ToMessageBody();

        return message;
    }
}
