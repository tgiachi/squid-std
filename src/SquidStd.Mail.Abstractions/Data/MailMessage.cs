namespace SquidStd.Mail.Abstractions.Data;

/// <summary>A received email, parsed from MIME.</summary>
public sealed record MailMessage(
    MailAddress From,
    IReadOnlyList<MailAddress> To,
    IReadOnlyList<MailAddress> Cc,
    string Subject,
    DateTime DateUtc,
    string MessageId,
    string? TextBody,
    string? HtmlBody,
    IReadOnlyList<MailAttachment> Attachments,
    byte[]? RawEml
);
