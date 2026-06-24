using SquidStd.Mail.Abstractions.Data;

namespace SquidStd.Mail.Abstractions.Data.Config;

/// <summary>Plain options for the SMTP sender (passed directly to AddMailSender).</summary>
public sealed class SmtpOptions
{
    /// <summary>SMTP server host.</summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>SMTP server port.</summary>
    public int Port { get; set; } = 587;

    /// <summary>Use SSL-on-connect (<c>true</c>) or STARTTLS-when-available (<c>false</c>).</summary>
    public bool UseSsl { get; set; }

    /// <summary>Login user name; when empty, authentication is skipped.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Login password.</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Default sender used when a message has no <c>From</c>.</summary>
    public MailAddress? DefaultFrom { get; set; }
}
