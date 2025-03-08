using DotnetSharedEntities;
using BusGateway.Entities;
using BusGateway.Extensions;
using DotnetMessageBusHub;
using System.Text.Json;

namespace BusGateway.Services;

public class PostMessageService
{
    private readonly IConfiguration configuration;
    private readonly ILogger<PostMessageService> logger;
    private readonly string enqueuerUri;

    public PostMessageService(IConfiguration configuration, ILogger<PostMessageService> logger)
    {
        this.configuration = configuration;
        this.logger = logger;
        enqueuerUri = ConfigurationHandler.GetEnqueuerUri(configuration);
    }

    public async Task<IResult> PostMessageToBus(HttpContext context, bool isTopicBased)
    {
        logger.LogInformation("Processing post message to bus request...");
        string messageFromBody = await ReadBodyFromContext(context);
        if(string.IsNullOrEmpty(messageFromBody))
        {
            logger.LogInformation("No message found in the request body");
            return Results.BadRequest("No message found in the request body");
        }

        IPostMessage postMessage = GeneratePostMessageFromRequest(messageFromBody, isTopicBased);
        if(postMessage == null)
        {
            logger.LogInformation("Failed to deserialize request body");
            return Results.BadRequest("Failed to deserialize request body");
        }

        return await PostToBus(postMessage, isTopicBased);
    }

    private async Task<string> ReadBodyFromContext(HttpContext context)
    {
        string body = string.Empty;
        if (context.Request.Body != null)
        {
            using (var reader = new StreamReader(context.Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }
        }
        return body;
    }

    private IPostMessage? GeneratePostMessageFromRequest(string messageFromBody, bool isTopicBased)
    {
        try 
        {
            if(isTopicBased)
            {
                return JsonSerializer.Deserialize<PostTopicMessage>(messageFromBody);
            }
            return JsonSerializer.Deserialize<PostIdMessage>(messageFromBody);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task<IResult> PostToBus(IPostMessage message, bool isTopicBased)
    {
        try
        {
            if(isTopicBased)
            {
                PostTopicMessage? postTopicMessage = message.ValidateTopicMessage();
                if(postTopicMessage is null)
                {
                    logger.LogInformation("Invalid topic message - Topic and payload can not be empty");
                    return Results.BadRequest($"Invalid topic message - Topic and payload can not be empty");
                }
                bool success = await MessageBusHub.PushMessageToBus(postTopicMessage.Topic, postTopicMessage.Payload, enqueuerUri);
                return success ? 
                        Results.Ok("Message pushed to bus") : 
                         Results.Problem(
                            detail: "Failed to push message to bus - bus is not responding",
                            statusCode: StatusCodes.Status500InternalServerError);
            }

            PostIdMessage? postIdMessage = message.ValidateIdMessage();
            if(postIdMessage is null)
            {
                logger.LogInformation("Invalid id message - Id and payload can not be empty");
                return Results.BadRequest($"Invalid id message - Id and payload can not be empty");
            }
            bool result = await MessageBusHub.PushMessageToBus(postIdMessage.Id, postIdMessage.Payload, enqueuerUri);
            return result ? 
                    Results.Ok("Message pushed to bus") : 
                        Results.Problem(
                        detail: "Failed to push message to bus - bus is not responding",
                        statusCode: StatusCodes.Status500InternalServerError);
        }
        catch(Exception ex)
        {
            logger.LogError($"Failed to post message to bus: {ex.Message}");
            return Results.BadRequest($"Failed to post message to bus: {ex.Message}");
        }
    }
}