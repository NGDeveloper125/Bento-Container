using DotnetSharedEntities;
using FluentAssertions;
using MessageBusDomain;
using MessageBusDomain.Entities;
using Microsoft.Extensions.Logging;

namespace MessageBusTests;

public class MessageBusUnitTests
{
    [Fact]
    public void HandleNewMessage_DontEmbusMessage_WhenMessageHasEmptyTopicAndId()
    {
        ILogger<MessageBus> logger = NSubstitute.Substitute.For<ILogger<MessageBus>>();
        var messageBus = new MessageBus(logger, null!);
        var messageWrapper = new MessageWrapper("", "payload", null);

        messageBus.HandleNewMessage(messageWrapper);
        QueueInfo queueInfo = messageBus.GetQueueInfo();

        queueInfo.QueueCount.Should().Be(0);
    }

    [Fact]
    public void HandleNewMessage_DontEmbusMessage_WhenMessageHasEmptyTopicAndNotValidId()
    {
        ILogger<MessageBus> logger = NSubstitute.Substitute.For<ILogger<MessageBus>>();
        var messageBus = new MessageBus(logger, null!);
        var messageWrapper = new MessageWrapper("", "payload", new Guid());


        messageBus.HandleNewMessage(messageWrapper);
        QueueInfo queueInfo = messageBus.GetQueueInfo();

        queueInfo.QueueCount.Should().Be(0);
    }

    [Fact]
    public void HandleNewMessage_DontEmbusMessage_WhenMessageHasEmptyPayload()
    {
        ILogger<MessageBus> logger = NSubstitute.Substitute.For<ILogger<MessageBus>>();
        var messageBus = new MessageBus(logger, null!);
        var messageWrapper = new MessageWrapper("TestTopic", "", null);

        messageBus.HandleNewMessage(messageWrapper);
        QueueInfo queueInfo = messageBus.GetQueueInfo();

        queueInfo.QueueCount.Should().Be(0);
    }

    
    [Fact]
    public void HandleNewMessage_EmbusMessage_WhenMessageIsValidWithTopic()
    {
        ILogger<MessageBus> logger = NSubstitute.Substitute.For<ILogger<MessageBus>>();
        var messageBus = new MessageBus(logger, null!);
        var messageWrapper = new MessageWrapper("TestTopic", "TestPayload", null);

        messageBus.HandleNewMessage(messageWrapper);
        QueueInfo queueInfo = messageBus.GetQueueInfo();

        queueInfo.QueueCount.Should().Be(1);
        queueInfo.Topics.Should().Contain("TestTopic");
    }

        [Fact]
    public void HandleNewMessage_EmbusMessage_WhenMessageIsValidWithId()
    {
        ILogger<MessageBus> logger = NSubstitute.Substitute.For<ILogger<MessageBus>>();
        var messageBus = new MessageBus(logger, null!);
        var messageWrapper = new MessageWrapper("", "TestPayload", Guid.NewGuid());

        messageBus.HandleNewMessage(messageWrapper);
        QueueInfo queueInfo = messageBus.GetQueueInfo();

        queueInfo.QueueCount.Should().Be(1);
        queueInfo.Ids.Should().Contain(messageWrapper.Id);
    }

    [Fact]
    public void HandleMessageRequest_ReturnIssueMessage_WhenMessageHasEmptyTopic()
    {
        ILogger<MessageBus> logger = NSubstitute.Substitute.For<ILogger<MessageBus>>();
        var queueMessage = new QueueMessage("TestTopic", "TestPayload", new Guid(), DateTime.Now);

        var queueMessages = new List<QueueMessage>
        {
            queueMessage
        };
        var messageBus = new MessageBus(logger, queueMessages);
        var requestMessage = new RequestMessage("", null);

        PulledMessage pulledMessage = messageBus.HandleRequestMessage(requestMessage);

        pulledMessage.SuccessfullyPulled.Should().Be(false);
        pulledMessage.Issue.Should().Be(PulledMessageIssue.NoTopicOrIdProvided);
    }

    [Fact]
    public void HandleMessageRequest_ReturnIssueMessage_WhenMessageTopicWasNotFound()
    {
        ILogger<MessageBus> logger = NSubstitute.Substitute.For<ILogger<MessageBus>>();
        var queueMessage = new QueueMessage("TestTopic", "TestPayload", new Guid(), DateTime.Now);

        var queueMessages = new List<QueueMessage>
        {
            queueMessage
        };
        var messageBus = new MessageBus(logger, queueMessages);
        var requestMessage = new RequestMessage("TestTopic1", null);

        PulledMessage pulledMessage = messageBus.HandleRequestMessage(requestMessage);

        pulledMessage.SuccessfullyPulled.Should().Be(false);
        pulledMessage.Issue.Should().Be(PulledMessageIssue.NoMessageFoundForThisTopic);
    }

    [Fact]
    public void HandleMessageRequest_ReturnIssueMessage_WhenThereIsNoMessageWithThisId()
    {
        ILogger<MessageBus> logger = NSubstitute.Substitute.For<ILogger<MessageBus>>();
        var queueMessage = new QueueMessage("TestTopic", "TestPayload", Guid.NewGuid(), DateTime.Now);

        var queueMessages = new List<QueueMessage>
        {
            queueMessage
        };
        var messageBus = new MessageBus(logger, queueMessages);
        var requestMessage = new RequestMessage(null!, Guid.NewGuid());

        PulledMessage pulledMessage = messageBus.HandleRequestMessage(requestMessage);

        pulledMessage.SuccessfullyPulled.Should().Be(false);
        pulledMessage.Issue.Should().Be(PulledMessageIssue.NoMessageFoundWithThisId);
    }
}