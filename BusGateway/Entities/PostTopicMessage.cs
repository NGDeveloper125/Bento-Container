
namespace BusGateway.Entities;

public class PostTopicMessage : IPostMessage
{
    public required string Topic { get; set; }
    public required string Payload { get; set; }
}