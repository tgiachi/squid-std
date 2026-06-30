namespace SquidStd.Mail.Abstractions.Exceptions;

/// <summary>Thrown when an outbound message cannot be sent.</summary>
public sealed class MailSendException : Exception
{
    /// <summary>Initializes the exception with a message and the underlying cause.</summary>
    public MailSendException(string message, Exception innerException)
        : base(message, innerException) { }
}
