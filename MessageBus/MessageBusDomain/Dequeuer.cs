using Microsoft.Extensions.Logging;
using MessageBusDomain.Entities;
using System.Text;
using System.Text.Json;
using NetMQ.Sockets;
using NetMQ;
using System.Collections.Concurrent;
using DotnetSharedEntities;

namespace MessageBusDomain;

public class Dequeuer(DequeuerInfo dequeuerInfo, MessageBus messageBus, ILogger<Dequeuer> logger)
{
    private readonly MessageBus messageBus = messageBus;
    private readonly DequeuerInfo dequeuerInfo = dequeuerInfo;
    private readonly ILogger<Dequeuer> logger = logger;
    private readonly ConcurrentDictionary<RoutingKey, TaskCompletionSource<PulledMessage>> requestCompletionSources = new();

    public void Run(CancellationToken cancellationToken)
    {
        using var socket = new RouterSocket($"{dequeuerInfo.Address.AddressString}:{dequeuerInfo.Port.PortNumber}");
        logger.LogInformation("dequeuer is now listning for messages");
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var routingKey = new RoutingKey();
                if (socket.TryReceiveRoutingKey(TimeSpan.FromSeconds(1), ref routingKey))
                {
                    logger.LogDebug("New request message reseived");
                    var clientAddress = socket.ReceiveFrameBytes();
                    var message = socket.ReceiveFrameBytes();
                    var completionSource = new TaskCompletionSource<PulledMessage>();
                    requestCompletionSources[routingKey] = completionSource;

                    Task.Run(() =>
                    {
                        var pulledMessage = HandleNewRequestMessage(message);
                        completionSource.SetResult(pulledMessage);
                    }, cancellationToken);

                    completionSource.Task.ContinueWith(task =>
                    {
                        var pulledMessage = task.Result;
                        socket.SendMoreFrame(routingKey);
                        socket.SendMoreFrameEmpty();
                        socket.SendFrame(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(pulledMessage)));
                        requestCompletionSources.TryRemove(routingKey, out _);
                        logger.LogDebug("Request message handled and response sent");
                    }, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to handle request message");
            }
        }
    }

    public PulledMessage HandleNewRequestMessage(byte[] message)
    {
        RequestMessage? requestMessage = null;
        try
        {
            string serializedMessage = Encoding.UTF8.GetString(message);
            requestMessage = JsonSerializer.Deserialize<RequestMessage>(serializedMessage);
        }
        catch
        {
            logger.LogDebug("Failed to deserialize message");
        }
        if (requestMessage is null)
        {
            logger.LogDebug("Message was not valid");
            return new PulledMessage(false, null!, PulledMessageIssue.FailedToDeSerializeMessage);
        }
        return messageBus.HandleRequestMessage(requestMessage);
    }
}

