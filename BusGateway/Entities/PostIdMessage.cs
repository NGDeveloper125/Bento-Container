
namespace BusGateway.Entities;

public class PostIdMessage : IPostMessage
{
    public required Guid Id { get; set; }
    public required string Payload { get; set; }
}