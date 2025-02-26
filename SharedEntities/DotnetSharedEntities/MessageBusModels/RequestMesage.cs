namespace DotnetSharedEntities.MessageBusModels;

public record RequestMessage(string Topic, Guid? Id);