
using DotnetSharedEntities.ConfigurationModels;
using Microsoft.Extensions.Configuration;

namespace DotnetSharedEntities;

public class ConfigurationHandler
{

    public static Service? GetServiceFromConfiguration(IConfiguration configuration, string serviceName)
    {
        var servicesSection = configuration.GetSection("Services");

        foreach (var serviceSection in servicesSection.GetChildren())
        {
            var service = new Service
            {
                ServiceName = serviceSection["ProjectName"]!,
                ServiceEnvironment = serviceSection["ProjectType"]!,
                ServiceLocation = serviceSection["ProjectLocation"]!,
                // Create a subconfiguration for the Configuration section
                Configuration = serviceSection.GetSection("Configuration")
            };

            if (service.ServiceName == serviceName) return service;
        }

        return null;
    }

    public static string GetEnqueuerUri(IConfiguration configuration)
    {
        string? enqueuerAddress = configuration["MessageBus:Enqueuer:Address"]
                            ?? throw new ArgumentNullException("Failed to find enqueuer address for message bus");
        string? enqueuerPort = configuration["MessageBus:Enqueuer:Port"]
                                    ?? throw new ArgumentNullException("Failed to find enqueuer port for message bus");
        return $"tcp://{enqueuerAddress}:{enqueuerPort}";
    }

    public static string GetDequeuerUri(IConfiguration configuration)
    {
        string? dequeuerAddress = configuration["MessageBus:Dequeuer:Address"]
                            ?? throw new ArgumentNullException("Failed to find enqueuer address for message bus");
        string? dequeuerPort = configuration["MessageBus:Dequeuer:Port"]
                                    ?? throw new ArgumentNullException("Failed to find enqueuer port for message bus");
        return $"tcp://{dequeuerAddress}:{dequeuerPort}";
    }
}
