using BentoContainerManager.Handlers;
using BentoContainerManager.Extensions;
using BentoContainerManager.Models;
using BentoContainerManager.Services;
using System.CommandLine;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BentoContainerManager;

public class Program
{
    private static string configFilePath = Path.GetFullPath(@"..\BentoConfiguration.json");
    private static Container container = null;

    public static async Task Main(string[] args)
    {
        var rootCommand = new RootCommand("Bento Container CLI");
        
        var buildCommand = new Command("build", "build Bento Container");
        var upCommand = new Command("up", "Start Bento Container");
        var downCommand = new Command("down", "Stop Bento Container");


        rootCommand.AddCommand(buildCommand);
        rootCommand.AddCommand(upCommand);
        rootCommand.AddCommand(downCommand);

        buildCommand.SetHandler(async () =>
        {
            await GenerateContainer();
            await BuildContainer();
        });

        upCommand.SetHandler(async () => 
        {
            await GenerateContainer();
            await BuildContainer();
            await StartContainer();
        });

        await rootCommand.InvokeAsync(args);
    }

    private static async Task GenerateContainer()
    {
        container = await GetContainerConfig();
        if(!container.IsValid())
        {
            Console.Error.WriteLine("Invalid container configuration");
            return;
        }
        container = await DockerfileManager.RegisterContainer(container);
    }

    private static async Task BuildContainer()
    {
       await DockerfileManager.GenerateDockerfile(container);
       await ContainerManager.BuildContainer(container);
    }

        private static async Task StartContainer()
    {
        container.Volumes ??= new List<VolumeMount>();
        
        // Create host log directory with full permissions
        var hostLogPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "logs"));
        Directory.CreateDirectory(hostLogPath);

        // Clear existing volume mounts to avoid duplicates
        container.Volumes.Clear();
        
        // Add log volume mount with specific configuration
        container.Volumes.Add(new VolumeMount 
        {
            Source = hostLogPath,
            Target = "/app/Services/MessageBusHost/logs",
            Mode = "rw"
        });

        // Configure detailed logging
        container.Environment ??= new Dictionary<string, string>();
        container.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        container.Environment["DOTNET_CONSOLE_LOG_LEVEL"] = "Information";
        container.Environment["ASPNETCORE_LOGGING__CONSOLE__DISABLECOLORS"] = "true";
        container.Environment["ASPNETCORE_LOGGING__CONSOLE__FORMAT"] = "Detailed";
        container.Environment["ASPNETCORE_LOGGING__FILE__PATH"] = "/app/Services/MessageBusHost/logs/messagebus.log";
        container.Environment["ASPNETCORE_LOGGING__FILE__ENABLED"] = "true";

        await ContainerManager.SpinUpContainer(container);
    }

    private async static Task<Container> GetContainerConfig()
    {
        try
        {
            var configJson = await File.ReadAllTextAsync(configFilePath);
            Container? container = JsonSerializer.Deserialize<Container>(configJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (container == null)
            {
                throw new Exception($"Failed to parse configuration file from {configFilePath}");
            }

            Console.WriteLine($"Processing {container.ContainerName} configuration");
            return container;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
            return null;
        }
    }
}