using DryIoc;
using SquidStd.Core.Interfaces.Events;
using SquidStd.Services.Core.Services;
using SquidStd.Mail.Abstractions.Data.Config;
using SquidStd.Mail.Abstractions.Interfaces;
using SquidStd.Mail.MailKit.Extensions;
using SquidStd.Mail.MailKit.Services;

namespace SquidStd.Tests.Mail;

public class MailSenderRegistrationExtensionsTests
{
    private static Container NewContainer()
    {
        var container = new Container();
        container.RegisterInstance<IEventBus>(new EventBusService());

        return container;
    }

    [Fact]
    public void AddMailSender_RegistersSender()
    {
        using var container = NewContainer();

        container.AddMailSender(new SmtpOptions { Host = "smtp.example.com", Port = 587 });

        Assert.IsType<MailKitMailSender>(container.Resolve<IMailSender>());
    }

    [Fact]
    public void AddMailSender_Throws_OnInvalidHostOrPort()
    {
        using var container = NewContainer();

        Assert.Throws<ArgumentException>(() => container.AddMailSender(new SmtpOptions { Host = string.Empty, Port = 587 }));
        Assert.Throws<ArgumentException>(() => container.AddMailSender(new SmtpOptions { Host = "h", Port = 0 }));
    }
}
