namespace SquidStd.Mail.Queue.Data.Config;

/// <summary>Plain options for the mail send queue (passed directly to AddMailQueue).</summary>
public sealed class MailQueueOptions
{
    /// <summary>Name of the queue outbound messages are published to.</summary>
    public string QueueName { get; set; } = "squidstd.mail.outbound";
}
