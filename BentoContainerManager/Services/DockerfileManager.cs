using BentoContainerManager.Handlers;
using BentoContainerManager.Models;

namespace BentoContainerManager.Services;

public class DockerfileManager
{

    public static async Task<Container> RegisterContainer(Container container)
    {
        // Read configuration
        container.RegisteredDependencies = DependenciesHandler.HandleDependencies(container.Services, container.BaseImage.Platform);
        IEnumerable<Project> handledServices = await ProjectsHandler.HandleProjects(container.Services);
        container.Services = handledServices.OfType<Service>().ToList();
        IEnumerable<Project> handledTestProjects = await ProjectsHandler.HandleProjects(container.Tests);
        container.Tests = handledTestProjects.OfType<TestProject>().ToList();

        // Log gathered information
        Console.WriteLine($"Container Name: {container.ContainerName}");
        Console.WriteLine($"Container Port: {container.ContainerPort}");
        Console.WriteLine($"Container Base Image: ");
        Console.WriteLine($" Platform - {container.BaseImage.Platform}");
        Console.WriteLine($" Image - {container.BaseImage.ImageType}");
        Console.WriteLine("Services:");
        container.Services.ForEach(svc => Console.WriteLine($" Service name - {svc.ProjectName} | Service location - {svc.ProjectLocation}"));
        Console.WriteLine("Tests:");
        container.Tests.ForEach(test => Console.WriteLine($" Test name - {test.ProjectName} | Test location - {test.ProjectLocation}"));
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
        
        if(container.Services.Any(service => service.ProjectName == "BusGateway"))
        {
            dockerfileContent.Add("RUN dotnet dev-certs https --trust");
        }

        dockerfileContent.Add("WORKDIR /app");
        dockerfileContent.Add("");

        // Copy published services
        foreach (var service in container.Services)
        {
            Console.WriteLine($"Adding {service.ProjectName} to docker file");
            var serviceName = service.ProjectName;
            dockerfileContent.Add($"# Copy {service.ProjectName}");
            dockerfileContent.Add($"COPY publish/{serviceName} /app/Services/{serviceName}");
            dockerfileContent.Add("");
        }

        // Copy published tests

        Console.WriteLine($"container tests: {container.Tests.Count}");
        foreach (var test in container.Tests)
        {
            Console.WriteLine($"Adding {test.ProjectName} to docker file");
            var testName = test.ProjectName;
            dockerfileContent.Add($"# Copy {test.ProjectName}");
            dockerfileContent.Add($"COPY publish/{testName} /app/Tests/{testName}");
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