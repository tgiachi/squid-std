namespace SquidStd.Mail.Abstractions.Data;

/// <summary>An attachment; <paramref name="Content" /> is populated only when attachment content is included.</summary>
/// <param name="FileName">Attachment file name.</param>
/// <param name="ContentType">MIME content type.</param>
/// <param name="Size">Content length in bytes.</param>
/// <param name="Content">Raw bytes, or <c>null</c> when content is excluded.</param>
public sealed record MailAttachment(string FileName, string ContentType, long Size, byte[]? Content);
