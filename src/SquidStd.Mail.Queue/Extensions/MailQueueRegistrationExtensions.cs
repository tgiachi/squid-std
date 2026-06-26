using DryIoc;
using SquidStd.Abstractions.Extensions.Services;
using SquidStd.Mail.Queue.Data.Config;
using SquidStd.Mail.Queue.Interfaces;
using SquidStd.Mail.Queue.Services;

namespace SquidStd.Mail.Queue.Extensions;

/// <summary>DryIoc registration helper for the mail send queue.</summary>
public static class MailQueueRegistrationExtensions
{
    extension(IContainer container)
    {
        /// <summary>
        ///     Registers the mail queue and its background consumer. Requires <c>IMessageQueue</c> (messaging) and
        ///     <c>IMailSender</c> (the SMTP sender) to be registered already.
        /// </summary>
        public IContainer AddMailQueue(MailQueueOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(container);

            container.RegisterInstance(options ?? new MailQueueOptions());
            container.Register<IMailQueue, MailQueue>(Reuse.Singleton);
            container.RegisterStdService<MailSendConsumerService, MailSendConsumerService>(100);

            return container;
        }
    }
}
