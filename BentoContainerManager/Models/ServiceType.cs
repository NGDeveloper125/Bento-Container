using System.Text.Json.Serialization;

namespace BentoContainerManager.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ServiceType
{
    dotnet
}