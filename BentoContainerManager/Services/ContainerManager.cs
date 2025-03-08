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
        if (Directory.Exists(infrastructurePath))
        {
            Directory.Delete(infrastructurePath, recursive: true);
        }
        Directory.CreateDirectory(infrastructurePath);
        Directory.CreateDirectory(Path.Combine(infrastructurePath, "Services"));
        Directory.CreateDirectory(Path.Combine(infrastructurePath, "Tests"));

        // Copy publish files to infrastructure
        foreach (var service in container.Services)
        {
            var servicePublishPath = Path.GetFullPath(Path.Combine("../", "publish", service.ProjectName));
            if (Directory.Exists(servicePublishPath))
            {
                CopyDirectory(servicePublishPath, Path.Combine(infrastructurePath, "Services", service.ProjectName));
                continue;
            }
            Console.WriteLine($"Service {service.ProjectName} not found in publish folder at path {servicePublishPath}");
        }

        // Copy test projects
        foreach (var test in container.Tests)
        {
            var testPublishPath = Path.GetFullPath(Path.Combine("../", "publish", test.ProjectName));
            if (Directory.Exists(testPublishPath))
            {
                CopyDirectory(testPublishPath, Path.Combine(infrastructurePath, "Tests", test.ProjectName));
                continue;
            }
            Console.WriteLine($"Test project {test.ProjectName} not found in publish folder at path {testPublishPath}");
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
            Arguments = $"run -d --name {containerName} -p 7229:7229 -p 7228:7228 -v \"{infrastructurePath}:/app\" {containerName}",
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

        // Give the services some time to start up
        await Task.Delay(5000);

        // Run tests after container is up
        foreach (var test in container.Tests)
        {
            Console.WriteLine($"\nRunning tests for {test.ProjectName}...\n");
            var testInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"exec {containerName} dotnet test /app/Tests/{test.ProjectName}/{test.ProjectName}.dll --logger \"console;verbosity=detailed\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var testProcess = Process.Start(testInfo);
            if (testProcess != null)
            {
                testProcess.OutputDataReceived += (sender, data) => 
                {
                    if (!string.IsNullOrEmpty(data.Data))
                        Console.WriteLine(data.Data);
                };
                testProcess.ErrorDataReceived += (sender, data) => 
                {
                    if (!string.IsNullOrEmpty(data.Data))
                        Console.WriteLine(data.Data);
                };

                testProcess.BeginOutputReadLine();
                testProcess.BeginErrorReadLine();
                await testProcess.WaitForExitAsync();

                if (testProcess.ExitCode != 0)
                {
                    Console.WriteLine($"Tests failed for {test.ProjectName} with exit code: {testProcess.ExitCode}");
                }
            }
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