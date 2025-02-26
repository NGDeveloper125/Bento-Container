using System.Text.Json;
using System.Text.Json.Serialization;

namespace BentoContainerManager.Models;

public class Service
{
    public string ServiceName { get; set; } = string.Empty;
    public ServiceType ServiceType { get; set; } = ServiceType.dotnet;
    public string ServiceLocation { get; set; } = string.Empty;
    public List<string> Dependencies { get; set; } = new();
    
    // Store any additional service-specific configuration as raw JSON
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalConfig { get; set; }
}