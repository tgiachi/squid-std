using MimeKit;
using SquidStd.Mail.Abstractions.Data;

namespace SquidStd.Mail.MailKit.Services;

/// <summary>Maps a MimeKit <see cref="MimeMessage" /> to a SquidStd <see cref="MailMessage" />.</summary>
public static class MimeMessageMapper
{
    /// <summary>Maps a MIME message; attachment bytes and the raw .eml are included only when requested.</summary>
    public static MailMessage Map(MimeMessage message, bool includeAttachmentContent, bool includeRawEml)
    {
        ArgumentNullException.ThrowIfNull(message);

        var from = message.From.Mailboxes.Select(ToAddress).FirstOrDefault() ?? new MailAddress(string.Empty, string.Empty);
        var to = message.To.Mailboxes.Select(ToAddress).ToArray();
        var cc = message.Cc.Mailboxes.Select(ToAddress).ToArray();
        var attachments = message.Attachments
                                 .OfType<MimePart>()
                                 .Select(part => ToAttachment(part, includeAttachmentContent))
                                 .ToArray();

        byte[]? rawEml = null;

        if (includeRawEml)
        {
            using var stream = new MemoryStream();
            message.WriteTo(stream);
            rawEml = stream.ToArray();
        }

        return new(
            from,
            to,
            cc,
            message.Subject ?? string.Empty,
            message.Date.UtcDateTime,
            message.MessageId ?? string.Empty,
            message.TextBody,
            message.HtmlBody,
            attachments,
            rawEml
        );
    }

    private static MailAddress ToAddress(MailboxAddress mailbox)
        => new(mailbox.Name ?? string.Empty, mailbox.Address);

    private static MailAttachment ToAttachment(MimePart part, bool includeContent)
    {
        byte[]? content = null;
        long size;

        using (var stream = new MemoryStream())
        {
            part.Content.DecodeTo(stream);
            size = stream.Length;

            if (includeContent)
            {
                content = stream.ToArray();
            }
        }

        var fileName = part.FileName ?? string.Empty;

        return new(fileName, part.ContentType.MimeType, size, content);
    }
}
