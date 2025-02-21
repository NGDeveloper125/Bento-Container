using MessageBusHost;
using Serilog;
using System.Diagnostics;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

string configFileLocation = Path.GetFullPath("../../BentoConfiguration.json");

//if (Debugger.IsAttached)
//{
//    configFileLocation = Path.GetFullPath("../../BentoConfiguration.json");
//}

builder.Configuration.AddJsonFile(configFileLocation);

Log.Logger = new LoggerConfiguration()
            .ReadFrom
            .Configuration(builder.Configuration)
            .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<LoggerManager>();

var host = builder.Build();

host.Run();
