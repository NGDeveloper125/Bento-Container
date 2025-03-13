using BusGateway.Services;

namespace BusGateway.Extensions;

public static class EndpointsMapper
{
    public static void MapEndpoints(this WebApplication app)
    {
        app.MapPost("/PostTopicMessage", async (PostMessageService postMessageService, HttpContext context) => {
            return await postMessageService.PostMessageToBus(context, true);
        });

        app.MapPost("/PostIdMessage", async (PostMessageService postMessageService, HttpContext context) => {
            return await postMessageService.PostMessageToBus(context, false);
        });

        app.MapGet("/HealthCheck", () => {
            return Results.Ok();
        });
    }
}