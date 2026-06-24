namespace SquidStd.Mail.Abstractions.Data;

/// <summary>An attachment to send with an outgoing message.</summary>
/// <param name="FileName">Attachment file name.</param>
/// <param name="ContentType">MIME content type, e.g. <c>application/pdf</c>.</param>
/// <param name="Content">Raw bytes.</param>
public sealed record OutgoingAttachment(string FileName, string ContentType, byte[] Content);
