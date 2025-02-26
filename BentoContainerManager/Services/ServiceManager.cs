using BentoContainerManager.Models;
using System.Diagnostics;

namespace BentoContainerManager.Services;

public static class ServiceManager
{
    private static readonly string PublishRootPath = Path.Combine(@"../", "publish");

    public static async Task<bool> PrepareServices(Service service)
    {
        try
        {
            // Ensure publish directory exists and clean it
            if (Directory.Exists(PublishRootPath))
            {
                Directory.Delete(PublishRootPath, recursive: true);
            }
            Directory.CreateDirectory(PublishRootPath);

            var servicePublishPath = Path.Combine(PublishRootPath, service.ServiceName);
            
            if (service.ServiceType == ServiceType.dotnet)
            {
                await PublishDotnetService(service, servicePublishPath);
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error preparing {service.ServiceName}: {ex.Message}");
            return false;
        }
    }

    private static async Task PublishDotnetService(Service service, string publishPath)
    {
        var projectPath = Path.GetFullPath(service.ServiceLocation + "/" + service.ServiceName + ".csproj");
        Console.WriteLine($"path: {projectPath}");
        if (!File.Exists(projectPath))
        {
            throw new FileNotFoundException($"Project file not found: {projectPath}");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"publish \"{projectPath}\" -c Release -o \"{publishPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new Exception($"Failed to start publish process for {service.ServiceName}");
        }

        // Capture output for logging
        process.OutputDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
        process.ErrorDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
        
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();
        
        if (process.ExitCode != 0)
        {
            throw new Exception($"Failed to publish {service.ServiceName}. Exit code: {process.ExitCode}");
        }

        Console.WriteLine($"Successfully published {service.ServiceName} to {publishPath}");
    }
}