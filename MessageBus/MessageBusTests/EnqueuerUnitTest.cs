using FluentAssertions;
using MessageBusDomain;
using Microsoft.Extensions.Logging;
using MessageBusDomain.Entities;
using System.Text.Json;
using System.Text;
using DotnetSharedEntities;

namespace MessageBusTests;

public class EnqueuerUnitTests
{
    private readonly MessageBus messageBus;
    private readonly Enqueuer enqueuer;

    public EnqueuerUnitTests()
    {
        ILogger<MessageBus> logger = NSubstitute.Substitute.For<ILogger<MessageBus>>();
        ILogger<Enqueuer> enqueuerLogger = NSubstitute.Substitute.For<ILogger<Enqueuer>>();
        messageBus = new MessageBus(logger, []);
        var pushSocketInfo = new EnqueuerInfo("0.0.0.0", "5555");
        enqueuer = new Enqueuer(pushSocketInfo, messageBus, enqueuerLogger);
    }

    [Fact]
    public void HandleNewMessage_DontAddMessageToQueue_WhenMessageHaveNoTopicOrId()
    {
        var messageWrapper = new MessageWrapper(null!, "payload", null!);
        string serializedMessage = JsonSerializer.Serialize(messageWrapper);
        byte[] buffer = Encoding.UTF8.GetBytes(serializedMessage);

        enqueuer.HandleNewMessage(buffer);
        QueueInfo queueInfo = messageBus.GetQueueInfo();

        queueInfo.QueueCount.Should().Be(0);
    }

    [Fact]
    public void HandleNewMessage_DontAddMessageToQueue_WhenMessageHaveNoPayload()
    {
        var messageWrapper = new MessageWrapper("Topic", null!, null!);
        string serializedMessage = JsonSerializer.Serialize(messageWrapper);
        byte[] buffer = Encoding.UTF8.GetBytes(serializedMessage);

        enqueuer.HandleNewMessage(buffer);
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

        enqueuer.HandleNewMessage(buffer);
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

        enqueuer.HandleNewMessage(buffer);
        await Task.Delay(1000);

        QueueInfo queueInfo = messageBus.GetQueueInfo();

        queueInfo.QueueCount.Should().Be(1);
    }

}