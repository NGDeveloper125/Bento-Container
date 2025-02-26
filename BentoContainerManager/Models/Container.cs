
namespace BentoContainerManager.Models;

public class Container
{
    public string ContainerName { get; set; } = string.Empty;
    public string ContainerPort { get; set; } = string.Empty;  
    public BaseImage BaseImage { get; set; } = new();
    public List<Service> Services { get; set; } = new();
    public List<string> RegisteredDependencies { get; set; } = new();
    public List<VolumeMount> Volumes { get; set; } = new List<VolumeMount>();
    public Dictionary<string, string> Environment { get; set; } = new Dictionary<string, string>();
}
