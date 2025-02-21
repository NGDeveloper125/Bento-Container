using FluentAssertions;
using MessageBusDomain;
using Microsoft.Extensions.Logging;

namespace MessageBusTests;

public class MessageBusUnitTests
{
    [Fact]
    public void HandleNewMessage_DontEmbusMessage_WhenMessageHasEmptyTopicAndId()
    {
        ILogger<MessageBus> logger = NSubstitute.Substitute.For<ILogger<MessageBus>>();
        var messageBus = new MessageBus(logger, null!);
        var messageWrapper = new MessageBusDomain.Entities.MessageWrapper("", "payload", null);

        messageBus.HandleNewMessage(messageWrapper);
        MessageBusDomain.Entities.QueueInfo queueInfo = messageBus.GetQueueInfo();

        queueInfo.QueueCount.Should().Be(0);
    }

    [Fact]
    public void HandleNewMessage_DontEmbusMessage_WhenMessageHasEmptyTopicAndNotValidId()
    {
        ILogger<MessageBus> logger = NSubstitute.Substitute.For<ILogger<MessageBus>>();
        var messageBus = new MessageBus(logger, null!);
        var messageWrapper = new MessageBusDomain.Entities.MessageWrapper("", "payload", new Guid());


        messageBus.HandleNewMessage(messageWrapper);
        MessageBusDomain.Entities.QueueInfo queueInfo = messageBus.GetQueueInfo();

        queueInfo.QueueCount.Should().Be(0);
    }

    [Fact]
    public void HandleNewMessage_DontEmbusMessage_WhenMessageHasEmptyPayload()
    {
        ILogger<MessageBus> logger = NSubstitute.Substitute.For<ILogger<MessageBus>>();
        var messageBus = new MessageBus(logger, null!);
        var messageWrapper = new MessageBusDomain.Entities.MessageWrapper("TestTopic", "", null);

        messageBus.HandleNewMessage(messageWrapper);
        MessageBusDomain.Entities.QueueInfo queueInfo = messageBus.GetQueueInfo();

        queueInfo.QueueCount.Should().Be(0);
    }

    
    [Fact]
    public void HandleNewMessage_EmbusMessage_WhenMessageIsValidWithTopic()
    {
        ILogger<MessageBus> logger = NSubstitute.Substitute.For<ILogger<MessageBus>>();
        var messageBus = new MessageBus(logger, null!);
        var messageWrapper = new MessageBusDomain.Entities.MessageWrapper("TestTopic", "TestPayload", null);

        messageBus.HandleNewMessage(messageWrapper);
        MessageBusDomain.Entities.QueueInfo queueInfo = messageBus.GetQueueInfo();

        queueInfo.QueueCount.Should().Be(1);
        queueInfo.Topics.Should().Contain("TestTopic");
    }

        [Fact]
    public void HandleNewMessage_EmbusMessage_WhenMessageIsValidWithId()
    {
        ILogger<MessageBus> logger = NSubstitute.Substitute.For<ILogger<MessageBus>>();
        var messageBus = new MessageBus(logger, null!);
        var messageWrapper = new MessageBusDomain.Entities.MessageWrapper("", "TestPayload", Guid.NewGuid());

        messageBus.HandleNewMessage(messageWrapper);
        MessageBusDomain.Entities.QueueInfo queueInfo = messageBus.GetQueueInfo();

        queueInfo.QueueCount.Should().Be(1);
        queueInfo.Ids.Should().Contain(messageWrapper.Id);
    }

    [Fact]
    public void HandleMessageRequest_ReturnIssueMessage_WhenMessageHasEmptyTopic()
    {
        ILogger<MessageBus> logger = NSubstitute.Substitute.For<ILogger<MessageBus>>();
        var queueMessage = new MessageBusDomain.Entities.QueueMessage("TestTopic", "TestPayload", new Guid(), DateTime.Now);

        var queueMessages = new List<MessageBusDomain.Entities.QueueMessage>
        {
            queueMessage
        };
        var messageBus = new MessageBus(logger, queueMessages);
        var requestMsssage = new MessageBusDomain.Entities.RequestMsssage("", null);

        MessageBusDomain.Entities.PulledMessage pulledMessage = messageBus.HandleRequestMessage(requestMsssage);

        pulledMessage.SuccessfullyPulled.Should().Be(false);
        pulledMessage.Issue.Should().Be(MessageBusDomain.Entities.PulledMessageIssue.NoTopicOrIdProvided);
    }

    [Fact]
    public void HandleMessageRequest_ReturnIssueMessage_WhenMessageTopicWasNotFound()
    {
        ILogger<MessageBus> logger = NSubstitute.Substitute.For<ILogger<MessageBus>>();
        var queueMessage = new MessageBusDomain.Entities.QueueMessage("TestTopic", "TestPayload", new Guid(), DateTime.Now);

        var queueMessages = new List<MessageBusDomain.Entities.QueueMessage>
        {
            queueMessage
        };
        var messageBus = new MessageBus(logger, queueMessages);
        var requestMsssage = new MessageBusDomain.Entities.RequestMsssage("TestTopic1", null);

        MessageBusDomain.Entities.PulledMessage pulledMessage = messageBus.HandleRequestMessage(requestMsssage);

        pulledMessage.SuccessfullyPulled.Should().Be(false);
        pulledMessage.Issue.Should().Be(MessageBusDomain.Entities.PulledMessageIssue.NoMessageFoundForThisTopic);
    }

    [Fact]
    public void HandleMessageRequest_ReturnIssueMessage_WhenThereIsNoMessageWithThisId()
    {
        ILogger<MessageBus> logger = NSubstitute.Substitute.For<ILogger<MessageBus>>();
        var queueMessage = new MessageBusDomain.Entities.QueueMessage("TestTopic", "TestPayload", Guid.NewGuid(), DateTime.Now);

        var queueMessages = new List<MessageBusDomain.Entities.QueueMessage>
        {
            queueMessage
        };
        var messageBus = new MessageBus(logger, queueMessages);
        var requestMsssage = new MessageBusDomain.Entities.RequestMsssage(null!, Guid.NewGuid());

        MessageBusDomain.Entities.PulledMessage pulledMessage = messageBus.HandleRequestMessage(requestMsssage);

        pulledMessage.SuccessfullyPulled.Should().Be(false);
        pulledMessage.Issue.Should().Be(MessageBusDomain.Entities.PulledMessageIssue.NoMessageFoundWithThisId);
    }
}