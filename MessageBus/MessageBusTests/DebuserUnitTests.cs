using FluentAssertions;
using MessageBusDomain;
using Microsoft.Extensions.Logging;
using MessageBusDomain.Entities;
using System.Text.Json;
using System.Text;

namespace MessageBusTests;

public class DebuserUnitTests
{
    private readonly MessageBus messageBus;
    private readonly Debuser debuser;

    public DebuserUnitTests()
    {
        ILogger<MessageBus> logger = NSubstitute.Substitute.For<ILogger<MessageBus>>();
        ILogger<Debuser> pullSocketLogger = NSubstitute.Substitute.For<ILogger<Debuser>>();
        messageBus = new MessageBus(logger, []);
        var pullSocketInfo = new DebuserInfo("0.0.0.0", "5555");
        debuser = new Debuser(pullSocketInfo, messageBus, pullSocketLogger);
    }


    [Fact]
    public void HandleNewRequestMessage_ReturnIssueMessage_WhenMessageFailedToDeserialized()
    {
        string serializedMessage = JsonSerializer.Serialize("requestMsssage");
        byte[] message = Encoding.UTF8.GetBytes(serializedMessage);

        PulledMessage pulledMessage = debuser.HandleNewRequestMessage(message);

        pulledMessage.SuccessfullyPulled.Should().BeFalse();
        pulledMessage.Issue.Should().Be(PulledMessageIssue.FailedToDeSerializeMessage);
    }

    [Fact]
    public void HandleNewRequestMessage_ReturnIssueMessage_WhenTopicAndIdAreNotValid()
    {
        var requestMsssage = new RequestMsssage("", null);
        string serializedMessage = JsonSerializer.Serialize(requestMsssage);
        byte[] message = Encoding.UTF8.GetBytes(serializedMessage);

        PulledMessage pulledMessage = debuser.HandleNewRequestMessage(message);

        pulledMessage.SuccessfullyPulled.Should().BeFalse();
        pulledMessage.Issue.Should().Be(PulledMessageIssue.NoTopicOrIdProvided);
    }


    [Fact]
    public void HandleNewRequestMessage_ReturnIssueMessage_WhenIdIsValidButNotExistInTheQueue()
    {
        Guid id = Guid.NewGuid();
        var requestMsssage = new RequestMsssage("", id);
        string serializedMessage = JsonSerializer.Serialize(requestMsssage);
        byte[] message = Encoding.UTF8.GetBytes(serializedMessage);

        PulledMessage pulledMessage = debuser.HandleNewRequestMessage(message);

        pulledMessage.SuccessfullyPulled.Should().BeFalse();
        pulledMessage.Issue.Should().Be(PulledMessageIssue.NoMessageFoundWithThisId);
    }


    [Fact]
    public void HandleNewRequestMessage_ReturnPulledMessage_WhenIdIsValidAndExistInQueue()
    {
        Guid id = Guid.NewGuid();
        var messageWrapper = new MessageWrapper("test", "payload", id);
        messageBus.HandleNewMessage(messageWrapper);
        var requestMsssage = new RequestMsssage("", id);
        string serializedMessage = JsonSerializer.Serialize(requestMsssage);
        byte[] message = Encoding.UTF8.GetBytes(serializedMessage);

        PulledMessage pulledMessage = debuser.HandleNewRequestMessage(message);

        pulledMessage.SuccessfullyPulled.Should().BeTrue();
        pulledMessage.Payload.Should().Be("payload");
    }


    [Fact]
    public void HandleNewRequestMessage_ReturnIssueMessage_WhenTopicIsValidButNotExistInQueue()
    {
        var requestMsssage = new RequestMsssage("topic", null);
        string serializedMessage = JsonSerializer.Serialize(requestMsssage);
        byte[] message = Encoding.UTF8.GetBytes(serializedMessage);

        PulledMessage pulledMessage = debuser.HandleNewRequestMessage(message);

        pulledMessage.SuccessfullyPulled.Should().BeFalse();
        pulledMessage.Issue.Should().Be(PulledMessageIssue.NoMessageFoundForThisTopic);
    }


    [Fact]
    public void HandleNewRequestMessage_ReturnPulledMessage_WhenTopicIsValidAndExistInQueue()
    {
        var messageWrapper = new MessageWrapper("Topic", "payload", null);
        messageBus.HandleNewMessage(messageWrapper);
        var requestMsssage = new RequestMsssage("Topic", null);
        string serializedMessage = JsonSerializer.Serialize(requestMsssage);
        byte[] message = Encoding.UTF8.GetBytes(serializedMessage);

        PulledMessage pulledMessage = debuser.HandleNewRequestMessage(message);

        pulledMessage.SuccessfullyPulled.Should().BeTrue();
        pulledMessage.Payload.Should().Be("payload");
    }
}