using DotnetSharedEntities;
using DotnetSharedEntities.ConfigurationModels;
using MessageBusHost;
using Serilog;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

string configFileLocation = Path.GetFullPath("../../BentoConfiguration.json");

builder.Configuration.AddJsonFile(configFileLocation);

Service? messageBushostConfiguration = ConfigurationHandler.GetServiceFromConfiguration(builder.Configuration, "MessageBusHost") 
                                       ?? throw new Exception("Failed to find MessageBusHost configuration");

Log.Logger = new LoggerConfiguration()
            .ReadFrom
            .Configuration(messageBushostConfiguration.Configuration)
            .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();
builder.Services.AddSingleton<Microsoft.Extensions.Logging.ILogger>(sp =>
    sp.GetRequiredService<ILoggerFactory>().CreateLogger("DefaultLogger"));
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();
