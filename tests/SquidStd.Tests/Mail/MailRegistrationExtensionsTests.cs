using DryIoc;
using SquidStd.Core.Interfaces.Events;
using SquidStd.Core.Interfaces.Timing;
using SquidStd.Mail.Abstractions.Data.Config;
using SquidStd.Mail.Abstractions.Interfaces;
using SquidStd.Mail.Abstractions.Types.Mail;
using SquidStd.Mail.MailKit.Extensions;
using SquidStd.Mail.MailKit.Services;
using SquidStd.Services.Core.Services;
using SquidStd.Services.Core.Services.Scheduling;
using SquidStd.Tests.Mail.Support;

namespace SquidStd.Tests.Mail;

public class MailRegistrationExtensionsTests
{
    [Fact]
    public void AddMail_Imap_RegistersImapReaderAndService()
    {
        using var container = NewContainer();

        container.AddMail(ValidOptions(MailProtocolType.Imap));

        Assert.IsType<ImapMailReader>(container.Resolve<IMailReader>());
        Assert.NotNull(container.Resolve<MailPollingService>());
        Assert.True(container.IsRegistered<TimerWheelPumpService>());
    }

    [Fact]
    public void AddMail_Pop3_RegistersPop3Reader()
    {
        using var container = NewContainer();

        container.AddMail(ValidOptions(MailProtocolType.Pop3));

        Assert.IsType<Pop3MailReader>(container.Resolve<IMailReader>());
    }

    [Fact]
    public void AddMail_Throws_OnInvalidHostOrPort()
    {
        using var container = NewContainer();

        Assert.Throws<ArgumentException>(() => container.AddMail(new MailOptions { Host = string.Empty, Port = 993 }));
        Assert.Throws<ArgumentException>(() => container.AddMail(new MailOptions { Host = "h", Port = 0 }));
    }

    private static Container NewContainer()
    {
        var container = new Container();
        container.RegisterInstance<IEventBus>(new EventBusService());
        container.RegisterInstance<ITimerService>(new FakeTimerService());

        return container;
    }

    private static MailOptions ValidOptions(MailProtocolType protocol)
    {
        return new MailOptions
            { Protocol = protocol, Host = "mail.example.com", Port = 993, Username = "u", Password = "p" };
    }
}
