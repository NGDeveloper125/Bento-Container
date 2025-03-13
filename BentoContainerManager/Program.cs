using BentoContainerManager.Handlers;
using BentoContainerManager.Extensions;
using BentoContainerManager.Models;
using BentoContainerManager.Services;
using Microsoft.Extensions.Configuration;
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
        Console.WriteLine("Starting to generate container...");
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
        Console.WriteLine("Getting container configuration...");
        try
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.GetFullPath("../BentoConfiguration.json"))
                .Build();

            var services = new List<Service>();
            var servicesSection = configuration.GetSection("Services");
            foreach (var serviceSection in servicesSection.GetChildren())
            {
                var service = new Service
                {
                    ProjectName = serviceSection["ProjectName"]!,
                    ProjectType = Enum.Parse<ProjectType>(serviceSection["ProjectType"]!),
                    ProjectEnvironment = Enum.Parse<ProjectEnvironment>(serviceSection["ProjectEnvironment"]!),
                    ProjectLocation = serviceSection["ProjectLocation"]!,
                    Dependencies = serviceSection["Dependencies"].Split(',').ToList(),
                    Configuration = serviceSection.GetSection("Configuration")
                };

                services.Add(service);
            }

            var tests = new List<TestProject>();
            var testsSection = configuration.GetSection("Tests");
            foreach (var testSection in testsSection.GetChildren())
            {
                var testProject = new TestProject
                {
                    ProjectName = testSection["ProjectName"]!,
                    ProjectType = Enum.Parse<ProjectType>(testSection["ProjectType"]!),
                    ProjectEnvironment = Enum.Parse<ProjectEnvironment>(testSection["ProjectEnvironment"]!),
                    ProjectLocation = testSection["ProjectLocation"]!,
                };

                tests.Add(testProject);
            }

            BaseImage? baseImage = new BaseImage()
            {
                Platform = Enum.Parse<Platform>(configuration["BaseImage:Platform"]!),
                ImageType = Enum.Parse<BaseImageType>(configuration["BaseImage:ImageType"]!),
                CustomBaseImage = configuration["BaseImage:CustomBaseImage"]
            };

            Container? container = new Container()
            {
                ContainerName = configuration["ContainerName"],
                ContainerPort = configuration["ContainerPort"],
                BaseImage = baseImage,
                Services = services,
                Tests = tests
            };

            Console.WriteLine($"Processing {container.ContainerName} configuration");
            Console.WriteLine($"Container contain: {container.Services.Count} services and {container.Tests.Count} tests");
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