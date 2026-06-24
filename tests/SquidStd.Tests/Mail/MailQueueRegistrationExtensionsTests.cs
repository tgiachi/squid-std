using DryIoc;
using SquidStd.Core.Interfaces.Events;
using SquidStd.Mail.Abstractions.Interfaces;
using SquidStd.Mail.Queue.Extensions;
using SquidStd.Mail.Queue.Interfaces;
using SquidStd.Mail.Queue.Services;
using SquidStd.Messaging.Extensions;
using SquidStd.Services.Core.Services;
using SquidStd.Tests.Mail.Support;

namespace SquidStd.Tests.Mail;

public class MailQueueRegistrationExtensionsTests
{
    [Fact]
    public void AddMailQueue_RegistersQueueAndConsumer()
    {
        var container = new Container();
        container.RegisterInstance<IEventBus>(new EventBusService());
        container.AddInMemoryMessaging();
        container.RegisterInstance<IMailSender>(new RecordingMailSender());

        container.AddMailQueue();

        Assert.IsType<MailQueue>(container.Resolve<IMailQueue>());
        Assert.NotNull(container.Resolve<MailSendConsumerService>());
    }
}
