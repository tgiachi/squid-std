using DryIoc;
using SquidStd.Mail.Abstractions.Data.Config;
using SquidStd.Mail.Abstractions.Interfaces;
using SquidStd.Mail.MailKit.Services;

namespace SquidStd.Mail.MailKit.Extensions;

/// <summary>DryIoc registration helper for the SMTP sender.</summary>
public static class MailSenderRegistrationExtensions
{
    /// <summary>Registers <see cref="SmtpOptions" /> and <see cref="IMailSender" /> (MailKit SMTP).</summary>
    public static IContainer AddMailSender(this IContainer container, SmtpOptions options)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Host);

        if (options.Port <= 0)
        {
            throw new ArgumentException("Port must be positive.", nameof(options));
        }

        container.RegisterInstance(options);
        container.Register<IMailSender, MailKitMailSender>(Reuse.Singleton);

        return container;
    }
}
