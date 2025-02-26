using System.Text.Json.Serialization;

namespace BentoContainerManager.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Platform
{
    Linux,
    Windows
}