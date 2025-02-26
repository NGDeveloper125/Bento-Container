using BentoContainerManager.Services;
using BentoContainerManager.Models;

namespace BentoContainerManager.Handlers;

public class ServicesHandler
{
    public static async Task<List<Service>> HandleServices(IEnumerable<Service> services)
    {
        List<Service> registeredServices = new List<Service>();
        foreach (var service in services)
        {
            if (string.IsNullOrEmpty(service.ServiceName))
            {
                Console.WriteLine($"Service missing a name and will be removed");
                continue;
            }

            if(string.IsNullOrEmpty(service.ServiceLocation))
            {
                Console.WriteLine($"Service {service.ServiceName} missing a location and will be removed");
                continue;
            }        

            if(!Directory.Exists(Path.GetFullPath(service.ServiceLocation)) || Directory.GetFiles(Path.GetFullPath(service.ServiceLocation)).Length <= 0)
            {
                Console.WriteLine($"Service {service.ServiceName} missing a location or file could not be found at {Path.GetFullPath(service.ServiceLocation)} - service will be removed");
                continue;
            }

            if(!await ServiceManager.PrepareServices(service))
            {
                Console.WriteLine($"Service {service.ServiceName} could not be prepared - service will be removed");
                continue;
            }

            registeredServices.Add(service);
            Console.WriteLine($"Service {service.ServiceName} successfully registered");
        }
        return registeredServices;
    }
}