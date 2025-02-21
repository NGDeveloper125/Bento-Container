
namespace MessageBusDomain.Entities;

public record QueueMessage(string Topic, string Payload, Guid? Id, DateTime EmbusTime);