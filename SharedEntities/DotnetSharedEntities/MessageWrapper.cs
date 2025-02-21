
namespace DotnetSharedEntities;

public record MessageWrapper(string Topic, string Payload, Guid? Id);