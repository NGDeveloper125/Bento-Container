using BentoContainerManager.Handlers;
using BentoContainerManager.Entities;
using BentoContainerManager.Extensions;
using BentoContainerManager.Models;
using BentoContainerManager.Services;
using System.CommandLine;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;

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
        OperationSystem operationSystem = IdentifyOperationSystem();
        Console.WriteLine("Starting to generate container...");
        container = await ConfigurationHandler.GetContainerFromConfig(operationSystem);
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

    private static OperationSystem IdentifyOperationSystem()
    {
        if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.WriteLine("Running on Windows");
            return OperationSystem.Windows;
        }
        if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Console.WriteLine("Running on Linux");
            return OperationSystem.Linux;
        }
        throw new Exception("Unsupported operating system");
    }
}