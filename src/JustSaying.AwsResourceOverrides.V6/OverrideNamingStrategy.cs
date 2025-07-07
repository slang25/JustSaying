using JustSaying.AwsTools.QueueCreation;

namespace JustSaying.AwsResourceOverrides;

public sealed class OverrideNamingStrategy : INamingStrategy
{
    private readonly INamingStrategy _inner;
    private readonly Dictionary<Type, string> _queueNameOverrides;
    private readonly Dictionary<Type, string> _topicNameOverrides;

    public OverrideNamingStrategy(INamingStrategy inner,
        Dictionary<Type, string> queueNameOverrides, Dictionary<Type, string> topicNameOverrides)
    {
        _inner = inner;
        _queueNameOverrides = queueNameOverrides;
        _topicNameOverrides = topicNameOverrides;
    }

    public string GetTopicName(string topicName, Type messageType)
    {
        if (_topicNameOverrides.TryGetValue(messageType, out string? topicNameOverride))
        {
            return topicNameOverride[(topicNameOverride.LastIndexOf(':') + 1)..];
        }
        return _inner.GetTopicName(topicName, messageType);
    }

    public string GetQueueName(SqsReadConfiguration sqsConfig, Type messageType)
    {
        if (_queueNameOverrides.TryGetValue(messageType, out string? queueNameOverride))
        {
            return queueNameOverride[(queueNameOverride.LastIndexOf('/') + 1)..];
        }
        return _inner.GetQueueName(sqsConfig, messageType);
    }
}
