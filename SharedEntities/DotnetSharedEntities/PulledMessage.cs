
namespace DotnetSharedEntities;

public record PulledMessage(bool SuccessfullyPulled, string Payload, PulledMessageIssue Issue);