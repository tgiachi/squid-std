using SquidStd.Mail.Abstractions.Types.Mail;

namespace SquidStd.Mail.Abstractions.Data.Config;

/// <summary>Plain options for a single mailbox poller (not a config section — passed directly to AddMail).</summary>
public sealed class MailOptions
{
    /// <summary>Retrieval protocol.</summary>
    public MailProtocolType Protocol { get; set; } = MailProtocolType.Imap;

    /// <summary>Mail server host.</summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>Mail server port.</summary>
    public int Port { get; set; } = 993;

    /// <summary>Use an SSL/TLS connection.</summary>
    public bool UseSsl { get; set; } = true;

    /// <summary>Login user name.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Login password.</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>IMAP folder to poll.</summary>
    public string Folder { get; set; } = "INBOX";

    /// <summary>Seconds between polls.</summary>
    public int PollIntervalSeconds { get; set; } = 60;

    /// <summary>Mark IMAP messages as seen after a successful publish.</summary>
    public bool MarkAsSeen { get; set; } = true;

    /// <summary>Delete POP3 messages after download.</summary>
    public bool DeleteAfterDownload { get; set; }

    /// <summary>Include attachment bytes in the message.</summary>
    public bool IncludeAttachmentContent { get; set; } = true;

    /// <summary>Include the full raw .eml bytes in the message.</summary>
    public bool IncludeRawEml { get; set; }

    /// <summary>Maximum messages fetched per poll.</summary>
    public int MaxMessagesPerPoll { get; set; } = 50;
}
