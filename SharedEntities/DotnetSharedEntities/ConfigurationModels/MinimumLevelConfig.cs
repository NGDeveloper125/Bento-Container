
namespace DotnetSharedEntities.ConfigurationModels;

public class MinimumLevelConfig
{
    public string Default { get; set; } = string.Empty;
    public OverrideConfig Override { get; set; } = null!;
}