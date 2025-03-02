using System.Text.Json.Serialization;

namespace BentoContainerManager.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProjectType
{
    service,
    test
}