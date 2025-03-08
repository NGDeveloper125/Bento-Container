
using BusGateway.Entities;

namespace BusGateway.Extensions;

public static class PostMessageExtensions
{
    public static PostTopicMessage? ValidateTopicMessage(this IPostMessage message)
    {
        PostTopicMessage? postTopicMessage = message as PostTopicMessage;
        if(postTopicMessage is null)
        {
            return null;
        }
        if(string.IsNullOrEmpty(postTopicMessage!.Topic) || string.IsNullOrEmpty(postTopicMessage.Payload))
        {
            return null;
        }
        return postTopicMessage;
    }

    public static PostIdMessage? ValidateIdMessage(this IPostMessage message)
    {
        PostIdMessage? postIdMessage = message! as PostIdMessage;
        if(postIdMessage is null)
        {
            return null;
        }
        if(string.IsNullOrEmpty(postIdMessage.Payload))
        {
            return null;
        }
        return postIdMessage;
    }
}