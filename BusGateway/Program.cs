using BusGateway.Extensions;
using BusGateway.Middleware;
using BusGateway.Services;
using DotnetSharedEntities;
using DotnetSharedEntities.ConfigurationModels;
using Serilog;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile(Path.GetFullPath("../../BentoConfiguration.json"))
    .Build();

Service? service = ConfigurationHandler.GetServiceFromConfiguration(configuration, "BusGateway");
if(service is null)
{
    throw new Exception("Configuration for BusGateway not found");
}

Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(service.Configuration)
            .CreateLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddConfiguration(configuration);
builder.Host.UseSerilog(Log.Logger);
builder.Services.AddSingleton<BasicAuthenticationService>();
builder.Services.AddScoped<PostMessageService>();
builder.Services.AddEndpointsApiExplorer();

string httpUri = builder.Configuration["Kestrel:EndPoints:Http:Url"]!;
string httpsUri = builder.Configuration["Kestrel:EndPoints:HttpsDefaultCert:Url"]!;
Log.Information("Starting up...");
Log.Information("Running on the following URIs:");
Log.Information($"- HTTP: {httpUri}");
Log.Information($"- HTTPS: {httpsUri}");

WebApplication app = builder.Build();

app.UseHttpsRedirection();
if(bool.Parse(service.Configuration["Authentication:Enabled"]!))
{
    Log.Information("Running with basic authentication service");
    app.UseMiddleware<BasicAuthenticationService>();
}

app.MapEndpoints();

app.Run();
