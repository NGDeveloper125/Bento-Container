namespace DotnetSharedEntities.MessageBusModels;

public record PulledMessage(bool SuccessfullyPulled, string Payload, PulledMessageIssue Issue);