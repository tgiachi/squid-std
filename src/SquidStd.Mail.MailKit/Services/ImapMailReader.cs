using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Serilog;
using SquidStd.Mail.Abstractions.Data;
using SquidStd.Mail.Abstractions.Data.Config;
using SquidStd.Mail.Abstractions.Interfaces;

namespace SquidStd.Mail.MailKit.Services;

/// <summary>IMAP <see cref="IMailReader" />: fetches unseen messages and marks them seen after mapping.</summary>
public sealed class ImapMailReader : IMailReader
{
    private readonly ILogger _logger = Log.ForContext<ImapMailReader>();
    private readonly MailOptions _options;

    public ImapMailReader(MailOptions options)
    {
        _options = options;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MailMessage>> FetchNewAsync(CancellationToken cancellationToken = default)
    {
        using var client = new ImapClient();
        var results = new List<MailMessage>();

        try
        {
            await client.ConnectAsync(
                _options.Host,
                _options.Port,
                _options.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable,
                cancellationToken);
            await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);

            var folder = await client.GetFolderAsync(_options.Folder, cancellationToken);
            await folder.OpenAsync(FolderAccess.ReadWrite, cancellationToken);

            var uids = await folder.SearchAsync(SearchQuery.NotSeen, cancellationToken);

            foreach (var uid in uids.Take(_options.MaxMessagesPerPoll))
            {
                try
                {
                    var mime = await folder.GetMessageAsync(uid, cancellationToken);
                    results.Add(MimeMessageMapper.Map(mime, _options.IncludeAttachmentContent, _options.IncludeRawEml));

                    if (_options.MarkAsSeen)
                    {
                        await folder.AddFlagsAsync(uid, MessageFlags.Seen, silent: true, cancellationToken);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.Error(ex, "Failed to map IMAP message {Uid}; leaving it unseen.", uid);
                }
            }
        }
        finally
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync(true, CancellationToken.None);
            }
        }

        return results;
    }
}
