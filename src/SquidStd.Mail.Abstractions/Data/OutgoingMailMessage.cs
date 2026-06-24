namespace SquidStd.Mail.Abstractions.Data;

/// <summary>An email to send. Build with an object initializer.</summary>
public sealed record OutgoingMailMessage
{
    /// <summary>Sender; falls back to <c>SmtpOptions.DefaultFrom</c> when null.</summary>
    public MailAddress? From { get; init; }

    /// <summary>Primary recipients (required, non-empty).</summary>
    public IReadOnlyList<MailAddress> To { get; init; } = [];

    /// <summary>Carbon-copy recipients.</summary>
    public IReadOnlyList<MailAddress> Cc { get; init; } = [];

    /// <summary>Blind carbon-copy recipients.</summary>
    public IReadOnlyList<MailAddress> Bcc { get; init; } = [];

    /// <summary>Subject line.</summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>Plain-text body, or null.</summary>
    public string? TextBody { get; init; }

    /// <summary>HTML body, or null.</summary>
    public string? HtmlBody { get; init; }

    /// <summary>Attachments.</summary>
    public IReadOnlyList<OutgoingAttachment> Attachments { get; init; } = [];
}
