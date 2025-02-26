
using DotnetSharedEntities.ConfigurationModels;
using Microsoft.Extensions.Configuration;

namespace DotnetSharedEntities;

public class ConfigurationHandler
{

    public static ServiceConfig? GetServiceFromConfiguration(IConfiguration configuration, string serviceName)
    {
        var servicesSection = configuration.GetSection("Services");
        var services = servicesSection.Get<List<ServiceConfig>>();

        return services is not null ?
            services.FirstOrDefault(s => s.ServiceName == serviceName) :
            throw new ArgumentNullException($"Failed to fetch {serviceName} configuration");
    }
}
