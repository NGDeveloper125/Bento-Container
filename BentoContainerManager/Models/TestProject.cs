using System.Text.Json;
using System.Text.Json.Serialization;

namespace BentoContainerManager.Models;

public class TestProject : Project
{
    public override string ProjectName { get; set; } = string.Empty;
    public override string ProjectLocation { get; set; } = string.Empty;
    public override ProjectType ProjectType { get; set; } = ProjectType.service;
    public override ProjectEnvironment ProjectEnvironment { get; set; } = ProjectEnvironment.dotnet;
    public override List<string> Dependencies { get; set; } = new();
    public Dictionary<string, JsonElement>? AdditionalConfig { get; set; }
}