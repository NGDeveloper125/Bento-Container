
namespace DotnetSharedEntities.ConfigurationModels;

public class ServiceConfig
{
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string ServiceLocation { get; set; } = string.Empty;
    public List<string> Dependencies { get; set; } = [];
    public EnqueuerConfig Enqueuer { get; set; } = null!;
    public DequeuerConfig Dequeuer { get; set; } = null!;
    public SerilogConfig Serilog { get; set; } = null!;
}