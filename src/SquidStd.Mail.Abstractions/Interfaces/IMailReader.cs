using SquidStd.Mail.Abstractions.Data;

namespace SquidStd.Mail.Abstractions.Interfaces;

/// <summary>Fetches new messages from a mailbox.</summary>
public interface IMailReader
{
    /// <summary>
    ///     Connects, fetches the new (unseen) messages, marks them seen / deletes them per options, disconnects,
    ///     and returns the parsed messages.
    /// </summary>
    Task<IReadOnlyList<MailMessage>> FetchNewAsync(CancellationToken cancellationToken = default);
}
