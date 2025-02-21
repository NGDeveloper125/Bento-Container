using System.Text;
using System.Text.Json;
using NetMQ;
using NetMQ.Sockets;
using DotnetSharedEntities;

namespace DotnetMessageBusHub;

public class MessageBusHub
{
    public async static Task PushMessageToBus<T>(T message, string topic, string enqueuerUri)
    {
        string serializedMessage = JsonSerializer.Serialize(message);
        MessageWrapper messageWrapper = new MessageWrapper(topic, serializedMessage, null);

        await SendMessage(messageWrapper, enqueuerUri);
    }
    
    public async static Task PushMessageToBus<T>(T message, Guid id, string enqueuerUri)
    {
        string serializedMessage = JsonSerializer.Serialize(message);
        MessageWrapper messageWrapper = new MessageWrapper(null!, serializedMessage, id);

        await SendMessage(messageWrapper, enqueuerUri);
    }

    private async static Task SendMessage(MessageWrapper message, string enqueuerUri)
    {
        using(var socket = new RequestSocket(enqueuerUri))
        {
            await Task.Delay(500);
            string serializedMessageWrapper = JsonSerializer.Serialize(message);
            socket.SendFrame(serializedMessageWrapper);
        }
    }

    public async static Task<PulledMessage> PullMessageFromBus(string topic, string enqueuerUri, CancellationToken cancellationToken)
    {
        using(var socket = new RequestSocket(enqueuerUri))
        {
            RequestMessage request = new RequestMessage(topic, null);
            string serializedRequest = JsonSerializer.Serialize(request);
            await Task.Delay(500);
            socket.SendFrame(serializedRequest);               

            while(!cancellationToken.IsCancellationRequested)
            {
                byte[] message;
                if(socket.TryReceiveFrameBytes(TimeSpan.FromSeconds(1), out message!))
                {
                    return TranslateByteToMessage(message);
                }
            }
            return new PulledMessage(false, string.Empty, PulledMessageIssue.NullMessage);
        }
    }

    public async static Task<PulledMessage> PullMessageFromBus(Guid id, string enqueuerUri, CancellationToken cancellationToken)
    {
        using(var socket = new RequestSocket(enqueuerUri))
        {
            RequestMessage request = new RequestMessage(null!, id);
            string serializedRequest = JsonSerializer.Serialize(request);
            await Task.Delay(500);
            socket.SendFrame(serializedRequest);               

            while(!cancellationToken.IsCancellationRequested)
            {
                byte[] message;
                if(socket.TryReceiveFrameBytes(TimeSpan.FromSeconds(1), out message!))
                {
                    return TranslateByteToMessage(message);
                }
            }
            return new PulledMessage(false, string.Empty, PulledMessageIssue.NullMessage);
        }
    }

    private static PulledMessage TranslateByteToMessage(byte[] message)
    {
        string serializedMessage = Encoding.UTF8.GetString(message);
        PulledMessage? pulledMessage = JsonSerializer.Deserialize<PulledMessage>(serializedMessage);
        if(pulledMessage != null) return pulledMessage;
        return new PulledMessage(false, string.Empty, PulledMessageIssue.NullMessage);
    }
}

