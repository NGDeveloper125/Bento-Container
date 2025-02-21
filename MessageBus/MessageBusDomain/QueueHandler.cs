using MessageBusDomain.Entities;

namespace MessageBusDomain;

public static class QueueHandler
{
    public static QueueMessage? GetNextMessageByTopic(List<QueueMessage> queue, string topic) =>
                                queue.Where(qm => qm.Topic == topic)
                                     .OrderBy(qm => qm.EmbusTime)
                                     .FirstOrDefault();


    public static QueueInfo GetQueueInfo(List<QueueMessage> queue) =>
                    new(queue.Count, GetUniqueTopics(queue), GetUniqueIds(queue));


    internal static QueueMessage? GetMessageById(List<QueueMessage> queue, Guid? id) =>
                                    queue.Where(qm => qm.Id == id).FirstOrDefault();

    private static List<string> GetUniqueTopics(List<QueueMessage> queue) =>
                                    queue.Distinct().Select(qm => qm.Topic).ToList();

    private static List<Guid?> GetUniqueIds(List<QueueMessage> queue) =>
                                    queue.Distinct().Select(qm => qm.Id).ToList();

    public static List<QueueMessage> RemoveMessageFromQueue(List<QueueMessage> queue, QueueMessage message)
    {
        queue.Remove(message);
        return [.. queue];
    }

}