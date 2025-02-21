
namespace DotnetSharedEntities;

public enum PulledMessageIssue
{
    NoIssue = 0,
    NoMessageFoundForThisTopic = 1,
    NoTopicOrIdProvided = 2,
    NoMessageFoundWithThisId = 3,
    FailedToDeSerializeMessage = 4,
    NullMessage = 5,
}