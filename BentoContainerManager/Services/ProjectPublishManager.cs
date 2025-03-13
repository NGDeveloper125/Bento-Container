using BentoContainerManager.Models;
using System.Diagnostics;

namespace BentoContainerManager.Services;

public static class ProjectPublishManager
{
    private static readonly string PublishRootPath = Path.Combine(@"../", "publish");
    private static bool currentIterationStarted = false;

    public static async Task<bool> PrepareProject(Project project)
    {
        try
        {
            // Ensure publish directory exists and clean it
            HandlePublishDirectory();

            var projectPublishPath = Path.Combine(PublishRootPath, project.ProjectName);
            
            if (project.ProjectEnvironment == ProjectEnvironment.dotnet)
            {
                await PublishDotnetProject(project, projectPublishPath);
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error preparing {project.ProjectName}: {ex.Message}");
            return false;
        }
    }

    private static async Task PublishDotnetProject(Project project, string publishPath)
    {
        if(project.ProjectName == "BusGateway")
        {
            HandleBusGateway(project);
        }
        var projectPath = Path.GetFullPath(project.ProjectLocation + "/" + project.ProjectName + ".csproj");
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
            throw new Exception($"Failed to start publish process for {project.ProjectName}");
        }

        // Capture output for logging
        process.OutputDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
        process.ErrorDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
        
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();
        
        if (process.ExitCode != 0)
        {
            throw new Exception($"Failed to publish {project.ProjectName}. Exit code: {process.ExitCode}");
        }

        Console.WriteLine($"Successfully published {project.ProjectName} to {publishPath}");
    }

    private static void HandlePublishDirectory()
    {
        if(currentIterationStarted == false)
        {
            if (Directory.Exists(PublishRootPath))
            {
                Directory.Delete(PublishRootPath, recursive: true);
            }
            Directory.CreateDirectory(PublishRootPath);
            currentIterationStarted = true;
        }
    }

    private static void HandleBusGateway(Project project)
    {
        try
        {
            Service busGatewayService = project as Service;
            string httpUrl = $"http://0.0.0.0:{busGatewayService.Configuration["HostSettings:HttpPort"]}";
            string httpsUrl = $"https://0.0.0.0:{busGatewayService.Configuration["HostSettings:HttpsPort"]}";
            string appSettingsContent = File.ReadAllText(Path.Combine(project.ProjectLocation, "appsettings.json"));
            string newAppSettingsContent = appSettingsContent.Replace("{httpUrl}", httpUrl).Replace("{httpsUrl}", httpsUrl);
            File.WriteAllText(Path.Combine(project.ProjectLocation, "appsettings.json"), newAppSettingsContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error preparing BusGateway appsettings: {ex.Message}");
        }
    }
}