using System;
using System.Collections.Generic;
using System.IO;
using BentoContainerManager.Entities;
using BentoContainerManager.Models;
using Microsoft.Extensions.Configuration;

namespace BentoContainerManager.Handlers;

public class ConfigurationHandler
{
    public async static Task<Container> GetContainerFromConfig()
    {
        Console.WriteLine("Getting container configuration...");
        string configFilePath = Path.GetFullPath("../BentoConfiguration.json");
        try
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile(configFilePath)
                .Build();

            var services = GetServicesFromConfig(configuration);
            var tests = GetTestsFromConfig(configuration);

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

    private static List<Service> GetServicesFromConfig(IConfiguration configuration)
    {
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
        return services;
    }

    private static List<TestProject> GetTestsFromConfig(IConfiguration configuration)
    {
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
        return tests;
    }
}