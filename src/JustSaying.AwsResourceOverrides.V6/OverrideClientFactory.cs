using System.Collections.Generic;
using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using JustSaying.AwsTools;

namespace JustSaying.AwsResourceOverrides;

public sealed class OverrideClientFactory : IAwsClientFactory
{
    private readonly IAwsClientFactory _innerClientFactory;
    private readonly IReadOnlyCollection<string> _readonlyTopics;
    private readonly IReadOnlyCollection<string> _readonlyQueues;

    public OverrideClientFactory(IAwsClientFactory innerClientFactory, IReadOnlyCollection<string> readonlyTopics, IReadOnlyCollection<string> readonlyQueues)
    {
        _innerClientFactory = innerClientFactory;
        _readonlyTopics = readonlyTopics;
        _readonlyQueues = readonlyQueues;
    }

    public IAmazonSimpleNotificationService GetSnsClient(RegionEndpoint region)
    {
        return new ReadOnlySnsClient(_innerClientFactory.GetSnsClient(region), _readonlyTopics.ToList());
    }

    public IAmazonSQS GetSqsClient(RegionEndpoint region)
    {
        return new ReadOnlySqsClient(_innerClientFactory.GetSqsClient(region), _readonlyQueues.ToList());
    }
}
