using Microsoft.Extensions.Logging;
using MessageBusDomain.Entities;
using DotnetSharedEntities.MessageBusModels;
namespace MessageBusDomain;

public class MessageBus
{
    private readonly ILogger logger;
    public List<QueueMessage> Queue { get; private set; }
    public MessageBus(ILogger logger, List<QueueMessage> previousMessages)
    {
        this.logger = logger;
        Queue = [];
        if (previousMessages != null)
        {
            Queue.AddRange(previousMessages);
        }
    }

    public void HandleNewMessage(MessageWrapper messageWrapper)
    {
        logger.LogDebug("Handling new push message");
        if (!messageWrapper.IsValid())
        {
            logger.LogDebug("Message was not valid");
            return;
        }
        if (messageWrapper.Id is not null && messageWrapper.Id.IsValidId())
        {
            QueueInfo queueInfo1 = QueueHandler.GetQueueInfo(Queue);
            if (queueInfo1.Ids.Contains(messageWrapper.Id))
            {
                logger.LogDebug("Message with this id already exists");
                return;
            }
        }
        logger.LogDebug("Adding message to queue");
        Queue.Add(messageWrapper.GenerateQueueMessage());
        QueueInfo queueInfo2 = QueueHandler.GetQueueInfo(Queue);
        logger.LogDebug($"Currently there are {queueInfo2.QueueCount} messages in the queue");
    }

    public PulledMessage HandleRequestMessage(RequestMessage requestMessage)
    {
        logger.LogDebug("Handling new request message");
        QueueMessage? queueMessage;
        if ((requestMessage.Topic == null || requestMessage.Topic == "") && requestMessage.Id == null)
        {
            logger.LogDebug("No topic or id provided");
            return new PulledMessage(false, null!, PulledMessageIssue.NoTopicOrIdProvided);
        }

        if (requestMessage.Id is not null)
        {
            queueMessage = QueueHandler.GetMessageById(Queue, requestMessage.Id);
            if (queueMessage != null)
            {
                logger.LogDebug("Message found by id");
                Queue = QueueHandler.RemoveMessageFromQueue(Queue, queueMessage);
                return new PulledMessage(true, queueMessage.Payload, PulledMessageIssue.NoIssue);
            }
            logger.LogDebug("No message found with this id");
            return new PulledMessage(false, null!, PulledMessageIssue.NoMessageFoundWithThisId);
        }

        queueMessage = QueueHandler.GetNextMessageByTopic(Queue, requestMessage.Topic!);

        if (queueMessage != null)
        {
            logger.LogDebug("Message found by topic");
            Queue = QueueHandler.RemoveMessageFromQueue(Queue, queueMessage);
            return new PulledMessage(true, queueMessage.Payload, PulledMessageIssue.NoIssue);
        }

        logger.LogDebug("No message found with this topic");
        return new PulledMessage(false, null!, PulledMessageIssue.NoMessageFoundForThisTopic);
    }

    public QueueInfo GetQueueInfo()
    {
        return QueueHandler.GetQueueInfo(Queue);
    }

}

