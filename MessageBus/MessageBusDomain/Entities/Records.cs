
namespace MessageBusDomain.Entities;

public record Address(string AddressString);
public record Port(int PortNumber);
public record MessageWrapper(string Topic, string Payload, Guid? Id);
public record QueueInfo(int QueueCount, List<string> Topics, List<Guid?> Ids);
public record QueueMessage(string Topic, string Payload, Guid? Id, DateTime EmbusTime);
public record RequestMsssage(string Topic, Guid? Id);
public record PulledMessage(bool SuccessfullyPulled, string Payload, PulledMessageIssue Issue);