using Microsoft.Extensions.Logging;
using MessageBusDomain.Entities;
using System.Text.Json;
using System.Text;
using NetMQ.Sockets;
using NetMQ;
using DotnetSharedEntities;

namespace MessageBusDomain;

public class Enqueuer(EnqueuerInfo enqueuerInfo, MessageBus messageBus, ILogger logger)
{
    private readonly MessageBus messageBus = messageBus;
    private readonly EnqueuerInfo enqueuerInfo = enqueuerInfo;
    private readonly ILogger logger = logger;

    public void Run(CancellationToken cancellationToken)
    {
        using var socket = new RouterSocket($"{enqueuerInfo.Address.AddressString}:{enqueuerInfo.Port.PortNumber}");
        logger.LogInformation("enqueuer is now listening for messages");
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var routingKey = new RoutingKey();
                if (socket.TryReceiveRoutingKey(TimeSpan.FromSeconds(1), ref routingKey))
                {
                    logger.LogDebug("New push message received");
                    string msg = socket.ReceiveFrameString();
                    byte[] message = socket.ReceiveFrameBytes();
                    HandleNewMessage(message);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while receiving new message");
            }
        }
    }
    public void HandleNewMessage(byte[] buffer)
    {
        Task.Run(() =>
        {
            try
            {
                string serializedMessage = Encoding.UTF8.GetString(buffer);
                MessageWrapper? messageWrapper = JsonSerializer.Deserialize<MessageWrapper>(serializedMessage);
                if (messageWrapper == null)
                {
                    logger.LogDebug("Failed to deserialize message");
                    return;
                }
                messageBus.HandleNewMessage(messageWrapper);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while handling new message");
            }
        });
    }
}

