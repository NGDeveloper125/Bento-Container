
namespace BentoContainerManager.Models;

public abstract class Project
{
    public virtual string ProjectName { get; set; } = string.Empty;
    public virtual string ProjectLocation { get; set; } = string.Empty;
    public virtual ProjectType ProjectType { get; set; } = ProjectType.service;
    public virtual ProjectEnvironment ProjectEnvironment { get; set; } = ProjectEnvironment.dotnet;
    public virtual List<string> Dependencies { get; set; } = new();
}