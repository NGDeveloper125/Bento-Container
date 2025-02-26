namespace BentoContainerManager.Models;

public class VolumeMount
{
    public string Source { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public string Mode { get; set; } = "rw";
}