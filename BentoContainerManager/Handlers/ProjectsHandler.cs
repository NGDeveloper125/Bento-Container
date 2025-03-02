using BentoContainerManager.Services;
using BentoContainerManager.Models;

namespace BentoContainerManager.Handlers;

public class ProjectsHandler
{
    public static async Task<IEnumerable<Project>> HandleProjects(IEnumerable<Project> projects)
    {
        List<Project> registeredProjects = new List<Project>();
        foreach (var project in projects)
        {
            if (string.IsNullOrEmpty(project.ProjectName))
            {
                Console.WriteLine($"Project missing a name and will be removed");
                continue;
            }

            if(string.IsNullOrEmpty(project.ProjectLocation))
            {
                Console.WriteLine($"Project {project.ProjectName} missing a location and will be removed");
                continue;
            }        

            if(!Directory.Exists(Path.GetFullPath(project.ProjectLocation)) || Directory.GetFiles(Path.GetFullPath(project.ProjectLocation)).Length <= 0)
            {
                Console.WriteLine($"Project {project.ProjectName} missing a location or file could not be found at {Path.GetFullPath(project.ProjectLocation)} - project will be removed");
                continue;
            }

            if(!await ProjectPublishManager.PrepareProject(project))
            {
                Console.WriteLine($"Project {project.ProjectName} could not be prepared - project will be removed");
                continue;
            }

            registeredProjects.Add(project);
            Console.WriteLine($"Project {project.ProjectName} successfully registered");
        }
        return registeredProjects;
    }
}