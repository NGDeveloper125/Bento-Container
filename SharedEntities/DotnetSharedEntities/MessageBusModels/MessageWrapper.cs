namespace DotnetSharedEntities.MessageBusModels;

public record MessageWrapper(string Topic, string Payload, Guid? Id);