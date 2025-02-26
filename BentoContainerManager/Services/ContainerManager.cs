using BentoContainerManager.Models;
using System.Diagnostics;

namespace BentoContainerManager.Services;

public class ContainerManager
{
    public static async Task BuildContainer(Container container)
    {
        string containerName = container.ContainerName.ToLower();
        var startInfo = new ProcessStartInfo
        {
            WorkingDirectory = Path.GetFullPath(@"../"),
            FileName = "docker",
            Arguments = $"build --no-cache -t {containerName} .",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new Exception("Failed to start docker build process");
        }

        process.OutputDataReceived += (sender, data) => 
        {
            if (!string.IsNullOrEmpty(data.Data))
                Console.WriteLine(data.Data);
        };
        process.ErrorDataReceived += (sender, data) => 
        {
            if (!string.IsNullOrEmpty(data.Data))
                Console.WriteLine(data.Data);
        };

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Docker build failed with exit code: {process.ExitCode}");
        }
    }

    public static async Task SpinUpContainer(Container container)
    {
        string containerName = container.ContainerName.ToLower();
        
        // Create the infrastructure folder structure
        var infrastructurePath = Path.GetFullPath(Path.Combine("../", "Container"));
        Directory.CreateDirectory(infrastructurePath);

        // Copy initial files if directory is empty
        if (!Directory.EnumerateFileSystemEntries(infrastructurePath).Any())
        {
            // Copy publish files to infrastructure
            var publishPath = Path.GetFullPath(Path.Combine("../", "publish", "MessageBusHost"));
            if (Directory.Exists(publishPath))
            {
                CopyDirectory(publishPath, Path.Combine(infrastructurePath, "Services", "MessageBusHost"));
            }

            // Copy configuration file
            File.Copy(
                Path.GetFullPath(Path.Combine("../", "BentoConfiguration.json")),
                Path.Combine(infrastructurePath, "BentoConfiguration.json"),
                true);

            // Copy start script
            File.Copy(
                Path.GetFullPath(Path.Combine("../", "start-services.sh")),
                Path.Combine(infrastructurePath, "start-services.sh"),
                true);
        }
        
        // First, ensure any existing container with the same name is removed
        var cleanupInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"rm -f {containerName}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        try
        {
            using var cleanup = Process.Start(cleanupInfo);
            await cleanup.WaitForExitAsync();
        }
        catch { } // Ignore cleanup errors

        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"run -d --name {containerName} -v \"{infrastructurePath}:/app\" {containerName}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new Exception("Failed to start docker run process");
        }

        process.OutputDataReceived += (sender, data) => 
        {
            if (!string.IsNullOrEmpty(data.Data))
                Console.WriteLine(data.Data);
        };
        process.ErrorDataReceived += (sender, data) => 
        {
            if (!string.IsNullOrEmpty(data.Data))
                Console.WriteLine(data.Data);
        };

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Docker run failed with exit code: {process.ExitCode}");
        }

        Console.WriteLine($"Container {containerName} is running in detached mode");
    }

      private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        Directory.CreateDirectory(destinationDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(destinationDir, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }

        foreach (var directory in Directory.GetDirectories(sourceDir))
        {
            var destDir = Path.Combine(destinationDir, Path.GetFileName(directory));
            CopyDirectory(directory, destDir);
        }
    }
}