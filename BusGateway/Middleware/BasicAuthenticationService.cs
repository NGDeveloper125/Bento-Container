
using DotnetSharedEntities;
using DotnetSharedEntities.ConfigurationModels;

namespace BusGateway.Middleware;

public class BasicAuthenticationService : IMiddleware
{
    private readonly IConfiguration configuration;
    private readonly ILogger<BasicAuthenticationService> logger;

    public BasicAuthenticationService(IConfiguration configuration, ILogger<BasicAuthenticationService> logger)
    {
        this.logger = logger;
        Service? service = ConfigurationHandler.GetServiceFromConfiguration(configuration, "BusGateway");
        if(service is null)
        {
            throw new Exception("Configuration for BusGateway not found");
        }
        this.configuration = service.Configuration;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        logger.LogInformation("Basic Authentication has been invoked");
        string? authCode = configuration["Authentication:AuthCode"];
        if (string.IsNullOrEmpty(authCode))
        {
            logger.LogError("AuthCode is not set");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal Server Error");
            return;
        }

        string? userAuthCode = context.Request.Headers["AuthCode"];
        if (string.IsNullOrEmpty(userAuthCode))
        {
            logger.LogInformation("Incoming request missing AuthCode");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("No AuthCode provided");
            return;
        }

        if (userAuthCode != authCode)
        {
            logger.LogInformation($"Incoming request wrong AuthCode: {userAuthCode}");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid AuthCode");
            return;
        }

        logger.LogInformation("Successfully authenticated requset");
        await next(context);
    }

}