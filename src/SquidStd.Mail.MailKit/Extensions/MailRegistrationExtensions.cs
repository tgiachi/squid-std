using DryIoc;
using SquidStd.Abstractions.Extensions.Config;
using SquidStd.Abstractions.Extensions.Services;
using SquidStd.Core.Data.Timing;
using SquidStd.Core.Interfaces.Timing;
using SquidStd.Mail.Abstractions.Data.Config;
using SquidStd.Mail.Abstractions.Interfaces;
using SquidStd.Mail.Abstractions.Types.Mail;
using SquidStd.Mail.MailKit.Services;
using SquidStd.Services.Core.Services.Scheduling;

namespace SquidStd.Mail.MailKit.Extensions;

/// <summary>DryIoc registration helpers for the mail poller.</summary>
public static class MailRegistrationExtensions
{
    extension(IContainer container)
    {
        /// <summary>
        /// Registers a single mailbox poller: the options, the protocol-specific <see cref="IMailReader" />, the
        /// polling service, and the timer-wheel pump (only if not already registered).
        /// </summary>
        public IContainer AddMail(MailOptions options)
        {
            ArgumentNullException.ThrowIfNull(container);
            ArgumentNullException.ThrowIfNull(options);
            ArgumentException.ThrowIfNullOrWhiteSpace(options.Host);

            if (options.Port <= 0)
            {
                throw new ArgumentException("Port must be positive.", nameof(options));
            }

            container.RegisterInstance(options);

            if (options.Protocol == MailProtocolType.Pop3)
            {
                container.Register<IMailReader, Pop3MailReader>(Reuse.Singleton);
            }
            else
            {
                container.Register<IMailReader, ImapMailReader>(Reuse.Singleton);
            }

            container.RegisterStdService<MailPollingService, MailPollingService>(100);

            if (!container.IsRegistered<ITimerWheelDriver>())
            {
                container.RegisterConfigSection("timerWheelPump", static () => new TimerWheelPumpConfig(), -90);
                container.RegisterStdService<TimerWheelPumpService, TimerWheelPumpService>(-1);
                container.RegisterMapping<ITimerWheelDriver, TimerWheelPumpService>();
            }

            return container;
        }
    }
}
