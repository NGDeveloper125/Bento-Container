using FluentAssertions;
using MessageBusDomain;
using Microsoft.Extensions.Logging;
using MessageBusDomain.Entities;
using System.Text.Json;
using System.Text;
using DotnetSharedEntities;
using DotnetSharedEntities.MessageBusModels;

namespace MessageBusTests;

public class DequeuerUnitTests
{
    private readonly MessageBus messageBus;
    private readonly Dequeuer dequeuer;

    public DequeuerUnitTests()
    {
        ILogger<MessageBus> logger = NSubstitute.Substitute.For<ILogger<MessageBus>>();
        ILogger<Dequeuer> pullSocketLogger = NSubstitute.Substitute.For<ILogger<Dequeuer>>();
        messageBus = new MessageBus(logger, []);
        var pullSocketInfo = new DequeuerInfo("0.0.0.0", "5555");
        dequeuer = new Dequeuer(pullSocketInfo, messageBus, pullSocketLogger);
    }


    [Fact]
    public void HandleNewRequestMessage_ReturnIssueMessage_WhenMessageFailedToDeserialized()
    {
        string serializedMessage = JsonSerializer.Serialize("requestMessage");
        byte[] message = Encoding.UTF8.GetBytes(serializedMessage);

        PulledMessage pulledMessage = dequeuer.HandleNewRequestMessage(message);

        pulledMessage.SuccessfullyPulled.Should().BeFalse();
        pulledMessage.Issue.Should().Be(PulledMessageIssue.FailedToDeSerializeMessage);
    }

    [Fact]
    public void HandleNewRequestMessage_ReturnIssueMessage_WhenTopicAndIdAreNotValid()
    {
        var requestMessage = new RequestMessage("", null);
        string serializedMessage = JsonSerializer.Serialize(requestMessage);
        byte[] message = Encoding.UTF8.GetBytes(serializedMessage);

        PulledMessage pulledMessage = dequeuer.HandleNewRequestMessage(message);

        pulledMessage.SuccessfullyPulled.Should().BeFalse();
        pulledMessage.Issue.Should().Be(PulledMessageIssue.NoTopicOrIdProvided);
    }


    [Fact]
    public void HandleNewRequestMessage_ReturnIssueMessage_WhenIdIsValidButNotExistInTheQueue()
    {
        Guid id = Guid.NewGuid();
        var requestMessage = new RequestMessage("", id);
        string serializedMessage = JsonSerializer.Serialize(requestMessage);
        byte[] message = Encoding.UTF8.GetBytes(serializedMessage);

        PulledMessage pulledMessage = dequeuer.HandleNewRequestMessage(message);

        pulledMessage.SuccessfullyPulled.Should().BeFalse();
        pulledMessage.Issue.Should().Be(PulledMessageIssue.NoMessageFoundWithThisId);
    }


    [Fact]
    public void HandleNewRequestMessage_ReturnPulledMessage_WhenIdIsValidAndExistInQueue()
    {
        Guid id = Guid.NewGuid();
        var messageWrapper = new MessageWrapper("test", "payload", id);
        messageBus.HandleNewMessage(messageWrapper);
        var requestMessage = new RequestMessage("", id);
        string serializedMessage = JsonSerializer.Serialize(requestMessage);
        byte[] message = Encoding.UTF8.GetBytes(serializedMessage);

        PulledMessage pulledMessage = dequeuer.HandleNewRequestMessage(message);

        pulledMessage.SuccessfullyPulled.Should().BeTrue();
        pulledMessage.Payload.Should().Be("payload");
    }


    [Fact]
    public void HandleNewRequestMessage_ReturnIssueMessage_WhenTopicIsValidButNotExistInQueue()
    {
        var requestMessage = new RequestMessage("topic", null);
        string serializedMessage = JsonSerializer.Serialize(requestMessage);
        byte[] message = Encoding.UTF8.GetBytes(serializedMessage);

        PulledMessage pulledMessage = dequeuer.HandleNewRequestMessage(message);

        pulledMessage.SuccessfullyPulled.Should().BeFalse();
        pulledMessage.Issue.Should().Be(PulledMessageIssue.NoMessageFoundForThisTopic);
    }


    [Fact]
    public void HandleNewRequestMessage_ReturnPulledMessage_WhenTopicIsValidAndExistInQueue()
    {
        var messageWrapper = new MessageWrapper("Topic", "payload", null);
        messageBus.HandleNewMessage(messageWrapper);
        var requestMessage = new RequestMessage("Topic", null);
        string serializedMessage = JsonSerializer.Serialize(requestMessage);
        byte[] message = Encoding.UTF8.GetBytes(serializedMessage);

        PulledMessage pulledMessage = dequeuer.HandleNewRequestMessage(message);

        pulledMessage.SuccessfullyPulled.Should().BeTrue();
        pulledMessage.Payload.Should().Be("payload");
    }
}