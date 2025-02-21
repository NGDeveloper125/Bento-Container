using FluentAssertions;
using MessageBusDomain;
using Microsoft.Extensions.Logging;
using MessageBusDomain.Entities;
using System.Text.Json;
using System.Text;

namespace MessageBusTests;

public class EmbuserUnitTests
{
    private readonly MessageBus messageBus;
    private readonly Embuser embuser;

    public EmbuserUnitTests()
    {
        ILogger<MessageBus> logger = NSubstitute.Substitute.For<ILogger<MessageBus>>();
        ILogger<Embuser> embuserLogger = NSubstitute.Substitute.For<ILogger<Embuser>>();
        messageBus = new MessageBus(logger, []);
        var pushSocketInfo = new EmbuserInfo("0.0.0.0", "5555");
        embuser = new Embuser(pushSocketInfo, messageBus, embuserLogger);
    }

    [Fact]
    public void HandleNewMessage_DontAddMessageToQueue_WhenMessageHaveNoTopicOrId()
    {
        var messageWrapper = new MessageWrapper(null!, "payload", null!);
        string serializedMessage = JsonSerializer.Serialize(messageWrapper);
        byte[] buffer = Encoding.UTF8.GetBytes(serializedMessage);

        embuser.HandleNewMessage(buffer);
        QueueInfo queueInfo = messageBus.GetQueueInfo();

        queueInfo.QueueCount.Should().Be(0);
    }

    [Fact]
    public void HandleNewMessage_DontAddMessageToQueue_WhenMessageHaveNoPayload()
    {
        var messageWrapper = new MessageWrapper("Topic", null!, null!);
        string serializedMessage = JsonSerializer.Serialize(messageWrapper);
        byte[] buffer = Encoding.UTF8.GetBytes(serializedMessage);

        embuser.HandleNewMessage(buffer);
        QueueInfo queueInfo = messageBus.GetQueueInfo();

        queueInfo.QueueCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleNewMessage_DontAddMessageToQueue_WhenMessageIdAlreadyExistInQueue()
    {
        Guid messageId = Guid.NewGuid();
        var messageWrapper = new MessageWrapper(null!, "payload", messageId);
        messageBus.HandleNewMessage(messageWrapper);
        string serializedMessage = JsonSerializer.Serialize(messageWrapper);
        byte[] buffer = Encoding.UTF8.GetBytes(serializedMessage);

        embuser.HandleNewMessage(buffer);
        await Task.Delay(1000);

        QueueInfo queueInfo = messageBus.GetQueueInfo();

        queueInfo.QueueCount.Should().Be(1);
    }

    [Fact]
    public async Task HandleNewMessage_MessageToQueue_WhenMessageIsValid()
    {
        var messageWrapper = new MessageWrapper(null!, "payload", Guid.NewGuid());
        string serializedMessage = JsonSerializer.Serialize(messageWrapper);
        byte[] buffer = Encoding.UTF8.GetBytes(serializedMessage);

        embuser.HandleNewMessage(buffer);
        await Task.Delay(1000);

        QueueInfo queueInfo = messageBus.GetQueueInfo();

        queueInfo.QueueCount.Should().Be(1);
    }

}