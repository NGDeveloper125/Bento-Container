using BentoContainerManager.Models;

namespace BentoContainerManager.Handlers;

public class DependenciesHandler
{
    private static readonly Dictionary<string, (string Windows, string Linux)> SupportedDependencies = new()
    {
        { "dotnet sdk", (
            Windows: "RUN powershell -Command Invoke-WebRequest 'https://dot.net/v1/dotnet-install.ps1' -OutFile 'dotnet-install.ps1'; " +
                     "./dotnet-install.ps1 -Channel {version} -InstallDir '/dotnet'",
            Linux: "RUN wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && " +
                   "dpkg -i packages-microsoft-prod.deb && " +
                   "apt-get update && " +
                   "apt-get install -y dotnet-sdk-{version}"
        )},
        { "dotnet runtime", (
            Windows: "RUN powershell -Command Invoke-WebRequest 'https://dot.net/v1/dotnet-install.ps1' -OutFile 'dotnet-install.ps1'; " +
                     "./dotnet-install.ps1 -Runtime dotnet -Channel {version} -InstallDir '/dotnet'",
            Linux: "RUN apt-get update && apt-get install -y dotnet-runtime-{version}"
        )},
        { "dotnet aspnet", (
            Windows: "RUN powershell -Command Invoke-WebRequest 'https://dot.net/v1/dotnet-install.ps1' -OutFile 'dotnet-install.ps1'; " +
                     "./dotnet-install.ps1 -Runtime aspnetcore -Channel {version} -InstallDir '/dotnet'",
            Linux: "RUN apt-get update && apt-get install -y aspnetcore-runtime-{version}"
        )}
    };

    private static readonly string[] ValidDotnetVersions = { "3.1", "5.0", "6.0", "7.0", "8.0", "9.0" };

    public static List<string> HandleDependencies(List<Service> services, Platform platform)
    {
        List<string> uniqueDependencies = GatherUniqueDependencies(services);
        var validDependencies = new List<string>();
        foreach (string dependency in uniqueDependencies)
        {
            var dependencyLower = dependency.ToLower();
            if (IsDotnetDependency(dependencyLower))
            {
                var version = GetDotnetVersion(dependency);
                if (!string.IsNullOrEmpty(version))
                {
                    var command = platform == Platform.Windows 
                        ? SupportedDependencies[dependencyLower].Windows.Replace("{version}", version)
                        : SupportedDependencies[dependencyLower].Linux.Replace("{version}", version);
                    validDependencies.Add((command));
                    Console.WriteLine($"Dotnet dependency {dependencyLower} {version} - successfully registered");
                }
                continue;
            }

            var customSource = GetCustomDependencySource(dependency);
            if (!string.IsNullOrEmpty(customSource))
            {
                validDependencies.Add(customSource);
                Console.WriteLine($"Custom dependency '{customSource}' - registered successfully");
                continue;
            }
            Console.WriteLine($"Unsupported dependency '{dependency}' - this dependency will be ignored");
        }

        return validDependencies;
    }

    private static List<string> GatherUniqueDependencies(List<Service> services)
    {
        return services
            .SelectMany(service => service.Dependencies)
            .Distinct()
            .ToList();
    }

    private static bool IsDotnetDependency(string dependency) =>
    dependency is "dotnet sdk" or "dotnet runtime" or "dotnet aspnet";
        
    private static string? GetDotnetVersion(string dependency)
    {
        Console.WriteLine($"Please specify the version of {dependency} - {string.Join(", ", ValidDotnetVersions)}");
        var version = Console.ReadLine();
        
        if (ValidDotnetVersions.Contains(version))
        {
            return version;
        }
        
        Console.WriteLine($"Invalid version of {dependency} - this dependency will be ignored");
        return null;
    }

    private static string? GetCustomDependencySource(string dependency)
    {
        Console.WriteLine($"Unsupported dependency '{dependency}' - please insert the dependency source for dockerfile or leave empty to ignore");
        var source = Console.ReadLine();
        return string.IsNullOrWhiteSpace(source) ? null : source;
    }
}