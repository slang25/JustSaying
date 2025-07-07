using Amazon;
using Amazon.SQS.Model;
using JustSaying.Messaging.MessageHandling;
using LocalSqsSnsMessaging;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Message = JustSaying.Models.Message;

namespace JustSaying.AwsResourceOverrides.Tests;

public class InfraTests
{
    [Fact]
    public async Task InterceptsInfraModifications()
    {
        var bus = new InMemoryAwsBus();
        using var sns = bus.CreateSnsClient();
        using var sqs = bus.CreateSqsClient();

        // Create infra
        var topicResponse = await sns.CreateTopicAsync("orderaccepted");
        var queueResponse = await sqs.CreateQueueAsync("customerorders");
        await sns.SubscribeQueueAsync(topicResponse.TopicArn, sqs, queueResponse.QueueUrl);

        Dictionary<Type, string> readonlyTopics = new()
        {
            [typeof(OrderAccepted)] = topicResponse.TopicArn // "arn:aws:sns:us-east-1:000000000000:orderaccepted"
        };

        Dictionary<Type, string> readonlyQueues = new ()
        {
            [typeof(OrderAccepted)] = queueResponse.QueueUrl // "https://sqs.us-east-1.amazonaws.com/000000000000/customerorders"
        };

        CreateMeABus.DefaultClientFactory = () => new OverrideClientFactory(new LocalAwsClientFactory(bus), readonlyTopics.Values, readonlyQueues.Values);

        IAmJustSayingFluently justSayingBus =
            CreateMeABus.WithLogging(NullLoggerFactory.Instance)
                .InRegion(RegionEndpoint.EUWest1.SystemName)
                .WithNamingOverrides(readonlyQueues, readonlyTopics)
                .WithSnsMessagePublisher<OrderAccepted>()
                .WithSqsTopicSubscriber()
                .IntoDefaultQueue()
                .WithMessageHandler<OrderAccepted>(new OrderNotifier());

        var topics = await sns.ListTopicsAsync();
        var queues = await sqs.ListQueuesAsync(new ListQueuesRequest());
        topics.Topics.Count.ShouldBe(1);
        queues.QueueUrls.Count.ShouldBe(2);
    }

    [Fact]
    public async Task DoesNotChangeExistingQueuePolicy()
    {
        // Arrange
        var bus = new InMemoryAwsBus();
        using var sns = bus.CreateSnsClient();
        using var sqs = bus.CreateSqsClient();

        var topicResponse = await sns.CreateTopicAsync("orderaccepted");
        var queueResponse = await sqs.CreateQueueAsync("customerorders");

        var queueAttributes = await sqs.GetQueueAttributesAsync(
            queueResponse.QueueUrl,
            ["QueueArn"]
        );
        string? queueArn = queueAttributes.QueueARN;

        // A custom policy that is different from what JustSaying would create
        var initialPolicy = $$"""
            {
              "Version": "2012-10-17",
              "Id": "MyCustomPolicy123",
              "Statement": [
                {
                  "Sid": "MyCustomStatement123",
                  "Effect": "Allow",
                  "Principal": "*",
                  "Action": "SQS:SendMessage",
                  "Resource": "{{queueArn}}"
                }
              ]
            }
            """;

        await sqs.SetQueueAttributesAsync(
            queueResponse.QueueUrl,
            new Dictionary<string, string> { { "Policy", initialPolicy } }
        );

        await sns.SubscribeQueueAsync(
            topicResponse.TopicArn,
            sqs,
            queueResponse.QueueUrl
        );

        string? policyBefore = (
            await sqs.GetQueueAttributesAsync(
                queueResponse.QueueUrl,
                ["Policy"]
            )
        ).Policy;

        Dictionary<Type, string> readonlyTopics =
            new() { [typeof(OrderAccepted)] = topicResponse.TopicArn };
        Dictionary<Type, string> readonlyQueues =
            new() { [typeof(OrderAccepted)] = queueResponse.QueueUrl };

        CreateMeABus.DefaultClientFactory = () =>
            new OverrideClientFactory(
                new LocalAwsClientFactory(bus),
                readonlyTopics.Values,
                readonlyQueues.Values
            );

        // Act
        IAmJustSayingFluently justSayingBus = CreateMeABus
            .WithLogging(NullLoggerFactory.Instance)
            .InRegion(RegionEndpoint.EUWest1.SystemName)
            .WithNamingOverrides(readonlyQueues, readonlyTopics)
            .WithSnsMessagePublisher<OrderAccepted>()
            .WithSqsTopicSubscriber()
            .IntoDefaultQueue()
            .WithMessageHandler<OrderAccepted>(new OrderNotifier());

        // Assert
        string? policyAfter = (
            await sqs.GetQueueAttributesAsync(
                queueResponse.QueueUrl,
                ["Policy"]
            )
        ).Policy;

        policyAfter.ShouldBe(policyBefore);
    }

    [Fact]
    public Task ThrowsIfReadOnlyInfraDoesNotExist()
    {
        var bus = new InMemoryAwsBus();

        Dictionary<Type, string> readonlyTopics = new()
        {
            [typeof(OrderAccepted)] = "arn:aws:sns:us-east-1:000000000000:missing-topic"
        };

        Dictionary<Type, string> readonlyQueues = [];

        CreateMeABus.DefaultClientFactory = () => new OverrideClientFactory(new LocalAwsClientFactory(bus), readonlyTopics.Values, readonlyQueues.Values);

        Assert.Throws<InvalidOperationException>(() =>
            {
                IAmJustSayingFluently justSayingBus =
                    CreateMeABus.WithLogging(NullLoggerFactory.Instance)
                        .InRegion(RegionEndpoint.EUWest1.SystemName)
                        .WithNamingOverrides(readonlyQueues, readonlyTopics)
                        .WithSnsMessagePublisher<OrderAccepted>()
                        .WithSqsTopicSubscriber()
                        .IntoDefaultQueue()
                        .WithMessageHandler<OrderAccepted>(new OrderNotifier());
            }
        );
        return Task.CompletedTask;
    }

    public class OrderAccepted : Message
    {
        public OrderAccepted(int orderId)
        {
            OrderId = orderId;
        }
        public int OrderId { get; private set; }
    }

    public class OrderNotifier : IHandlerAsync<OrderAccepted>
    {
        public Task<bool> Handle(OrderAccepted message)
        {
            return Task.FromResult(true);
        }
    }
}
