
namespace DotnetSharedEntities.ConfigurationModels;

public class WriteToConfig
{
    public string Name { get; set; } = string.Empty;
    public ArgsConfig Args { get; set; } = null!;
}