using MailKit.Net.Pop3;
using MailKit.Security;
using Serilog;
using SquidStd.Mail.Abstractions.Data;
using SquidStd.Mail.Abstractions.Data.Config;
using SquidStd.Mail.Abstractions.Interfaces;

namespace SquidStd.Mail.MailKit.Services;

/// <summary>POP3 <see cref="IMailReader" />: fetches messages not seen before (by UIDL), optionally deletes them.</summary>
public sealed class Pop3MailReader : IMailReader
{
    private readonly ILogger _logger = Log.ForContext<Pop3MailReader>();
    private readonly MailOptions _options;
    private readonly HashSet<string> _seenUids = new(StringComparer.Ordinal);
    private readonly Lock _sync = new();

    public Pop3MailReader(MailOptions options)
    {
        _options = options;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MailMessage>> FetchNewAsync(CancellationToken cancellationToken = default)
    {
        using var client = new Pop3Client();
        var results = new List<MailMessage>();

        try
        {
            await client.ConnectAsync(
                _options.Host,
                _options.Port,
                _options.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable,
                cancellationToken
            );
            await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);

            var uids = await client.GetMessageUidsAsync(cancellationToken);
            var taken = 0;

            for (var index = 0; index < uids.Count && taken < _options.MaxMessagesPerPoll; index++)
            {
                var uid = uids[index];

                lock (_sync)
                {
                    if (!_seenUids.Add(uid))
                    {
                        continue;
                    }
                }

                taken++;

                try
                {
                    var mime = await client.GetMessageAsync(index, cancellationToken);
                    results.Add(MimeMessageMapper.Map(mime, _options.IncludeAttachmentContent, _options.IncludeRawEml));

                    if (_options.DeleteAfterDownload)
                    {
                        await client.DeleteMessageAsync(index, cancellationToken);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.Error(ex, "Failed to map POP3 message {Uid}.", uid);

                    lock (_sync)
                    {
                        _seenUids.Remove(uid);
                    }
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
