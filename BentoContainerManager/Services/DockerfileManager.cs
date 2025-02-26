using BentoContainerManager.Handlers;
using BentoContainerManager.Models;

namespace BentoContainerManager.Services;

public class DockerfileManager
{

    public static async Task<Container> RegisterContainer(Container container)
    {
        // Read configuration
        container.RegisteredDependencies = DependenciesHandler.HandleDependencies(container.Services, container.BaseImage.Platform);
        container.Services = await ServicesHandler.HandleServices(container.Services);

        // Log gathered information
        Console.WriteLine($"Container Name: {container.ContainerName}");
        Console.WriteLine($"Container Port: {container.ContainerPort}");
        Console.WriteLine($"Container Base Image: ");
        Console.WriteLine($" Platform - {container.BaseImage.Platform}");
        Console.WriteLine($" Image - {container.BaseImage.ImageType}");
        Console.WriteLine("Services:");
        container.Services.ForEach(svc => Console.WriteLine($" Service name - {svc.ServiceName} | Service location - {svc.ServiceLocation}"));
        Console.WriteLine("Dependencies:");
        container.RegisteredDependencies.ForEach(dep => Console.WriteLine($"  - {dep}"));
        return container;
    }

    public static async Task GenerateDockerfile(Container container)
    {
        var dockerfileContent = new List<string>
        {
            $"FROM {container.BaseImage.GetImageName()}",
            $"EXPOSE {container.ContainerPort}",
            "",
            "# Install common dependencies",
            "RUN apt-get update && \\",
            "    apt-get install -y wget && \\",
            "    rm -rf /var/lib/apt/lists/* && \\",
            "    apt-get clean",
            ""
        };

        dockerfileContent.AddRange(container.RegisteredDependencies);
        dockerfileContent.Add("WORKDIR /app");
        dockerfileContent.Add("");

        // Copy published services
        foreach (var service in container.Services)
        {
            var serviceName = service.ServiceName;
            dockerfileContent.Add($"# Copy {service.ServiceName}");
            dockerfileContent.Add($"COPY publish/{serviceName} /app/Services/{serviceName}");
            dockerfileContent.Add("");
        }

        // copy configuration
        dockerfileContent.Add("COPY BentoConfiguration.json /app/");

        // Generate and add start script
        var startScriptName = await ScriptsManager.GenerateStartScript(container.Services, container.BaseImage.Platform);

        if (container.BaseImage.Platform == Platform.Linux)
        {
            dockerfileContent.Add($"COPY --chmod=755 start-services.sh /app/");
            dockerfileContent.Add($"ENTRYPOINT [\"/app/{startScriptName}\"]");
        }
        else
        {
            dockerfileContent.Add($"ENTRYPOINT [\"powershell\", \"-File\", \"/app/{startScriptName}\"]");
        }

        await File.WriteAllLinesAsync(Path.Combine(@"../", "Dockerfile"), dockerfileContent);
    }

    private static string NormalizePath(string path, Platform platform)
    {
        return platform == Platform.Windows 
            ? path.Replace("/", "\\") 
            : path.Replace("\\", "/");
    }
}