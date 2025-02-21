using System.Text.Json;
using MessageBusDomain.Entities;

namespace MessageBusHost;

public class RecoveryHandler
{
    public static void SaveQueueMessages(List<QueueMessage> messages, ILogger<RecoveryHandler> logger)
    {
        string serilizedMessages = JsonSerializer.Serialize(messages);
        try
        {
            File.WriteAllText(@".\QueueRecoveryFile.txt", serilizedMessages);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving messages to recovery file");
        }
    }

    public static List<QueueMessage> LoadQueueMessages(ILogger<RecoveryHandler> logger)
    {
        try
        {
            List<QueueMessage>? desilizedQueue = null;
            if (!File.Exists(@".\QueueRecoveryFile.txt"))
            {
                File.Create(@".\QueueRecoveryFile.txt");
                return [];
            }

            string serilizedMessages = File.ReadAllText(@".\QueueRecoveryFile.txt");
            if (!string.IsNullOrEmpty(serilizedMessages) || !string.IsNullOrWhiteSpace(serilizedMessages))
            {
                desilizedQueue = JsonSerializer.Deserialize<List<QueueMessage>>(serilizedMessages);
            }
            return desilizedQueue ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading messages from recovery file");
            return [];
        }
    }
}

