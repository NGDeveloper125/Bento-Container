using Microsoft.Extensions.Logging;
using MessageBusDomain.Entities;
namespace MessageBusDomain;

public class MessageBus
{
    private readonly ILogger<MessageBus> logger;
    public List<QueueMessage> Queue { get; private set; }
    public MessageBus(ILogger<MessageBus> logger, List<QueueMessage> previousMessages)
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

    public PulledMessage HandleRequestMessage(RequestMsssage requestMsssage)
    {
        logger.LogDebug("Handling new request message");
        QueueMessage? queueMessage;
        if ((requestMsssage.Topic == null || requestMsssage.Topic == "") && requestMsssage.Id == null)
        {
            logger.LogDebug("No topic or id provided");
            return new PulledMessage(false, null!, PulledMessageIssue.NoTopicOrIdProvided);
        }

        if (requestMsssage.Id is not null)
        {
            queueMessage = QueueHandler.GetMessageById(Queue, requestMsssage.Id);
            if (queueMessage != null)
            {
                logger.LogDebug("Message found by id");
                Queue = QueueHandler.RemoveMessageFromQueue(Queue, queueMessage);
                return new PulledMessage(true, queueMessage.Payload, PulledMessageIssue.NoIssue);
            }
            logger.LogDebug("No message found with this id");
            return new PulledMessage(false, null!, PulledMessageIssue.NoMessageFoundWithThisId);
        }

        queueMessage = QueueHandler.GetNextMessageByTopic(Queue, requestMsssage.Topic!);

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

