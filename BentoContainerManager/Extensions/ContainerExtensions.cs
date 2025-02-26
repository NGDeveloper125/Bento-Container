using BentoContainerManager.Models;

namespace BentoContainerManager.Extensions;

public static class ContainerExtensions
{
    public static bool IsValid(this Container self) 
    {
        if(self.ContainerName.Length <= 0)
        {
            Console.WriteLine("Container name can't be empty");
            return false;  
        } 

        if(self.ContainerPort.Length <= 0 || self.ContainerPort.Length > 5 || !int.TryParse(self.ContainerPort, out int port))
        {
            Console.WriteLine("Container port have to be a number between 0 and 65535");
            return false;
        }

        if(self.Services.Count <= 0)
        {
            Console.WriteLine("Container services can't be empty");
            return false;
        }

        if(self.BaseImage is null)
        {
            Console.WriteLine("Container failed to process base image");
            return false;
        }

        return true;
    }
}