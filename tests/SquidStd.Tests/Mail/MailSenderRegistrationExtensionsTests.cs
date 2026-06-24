using DryIoc;
using SquidStd.Core.Interfaces.Events;
using SquidStd.Mail.Abstractions.Interfaces;
using SquidStd.Mail.MailKit.Extensions;
using SquidStd.Mail.MailKit.Services;
using SquidStd.Services.Core.Services;

namespace SquidStd.Tests.Mail;

public class MailSenderRegistrationExtensionsTests
{
    [Fact]
    public void AddMailSender_RegistersSender()
    {
        using var container = NewContainer();

        container.AddMailSender(new() { Host = "smtp.example.com", Port = 587 });

        Assert.IsType<MailKitMailSender>(container.Resolve<IMailSender>());
    }

    [Fact]
    public void AddMailSender_Throws_OnInvalidHostOrPort()
    {
        using var container = NewContainer();

        Assert.Throws<ArgumentException>(() => container.AddMailSender(new() { Host = string.Empty, Port = 587 }));
        Assert.Throws<ArgumentException>(() => container.AddMailSender(new() { Host = "h", Port = 0 }));
    }

    private static Container NewContainer()
    {
        var container = new Container();
        container.RegisterInstance<IEventBus>(new EventBusService());

        return container;
    }
}
