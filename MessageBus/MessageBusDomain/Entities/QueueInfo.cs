
namespace MessageBusDomain.Entities;

public record QueueInfo(int QueueCount, List<string> Topics, List<Guid?> Ids);