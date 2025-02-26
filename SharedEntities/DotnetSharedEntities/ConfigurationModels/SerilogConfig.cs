
namespace DotnetSharedEntities.ConfigurationModels;

public class SerilogConfig
{
    public List<string> Using { get; set; } = [];
    public MinimumLevelConfig MinimumLevel { get; set; } = null!;
    public List<WriteToConfig> WriteTo { get; set; } = [];
    public List<string> Enrich { get; set; } = [];
}