
using Microsoft.Extensions.Configuration;

namespace DotnetSharedEntities.ConfigurationModels;

public class Service
{
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceEnvironment { get; set; } = string.Empty;
    public string ServiceLocation { get; set; } = string.Empty;
    public List<string> Dependencies { get; set; } = [];
    public IConfiguration Configuration { get; set; } = null!;
}