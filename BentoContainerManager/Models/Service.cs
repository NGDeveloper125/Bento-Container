using Microsoft.Extensions.Configuration;

namespace BentoContainerManager.Models;

public class Service : Project
{
    public override string ProjectName { get; set; } = string.Empty;
    public override string ProjectLocation { get; set; } = string.Empty;
    public override ProjectType ProjectType { get; set; } = ProjectType.service;
    public override ProjectEnvironment ProjectEnvironment { get; set; } = ProjectEnvironment.dotnet;
    public override List<string> Dependencies { get; set; } = new();
    public IConfiguration Configuration { get; set; } = null!;
}