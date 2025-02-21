using MessageBusHost;
using Serilog;
using System.Diagnostics;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

string configFileLocation = string.Empty;
configFileLocation = Path.GetFullPath("../../BentoConfiguration.json");
if (Debugger.IsAttached)
{
    configFileLocation = Path.GetFullPath("../../BentoConfiguration.json");
}

builder.Configuration.AddJsonFile(configFileLocation);

Log.Logger = new LoggerConfiguration()
            .ReadFrom
            .Configuration(builder.Configuration)
            .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();
