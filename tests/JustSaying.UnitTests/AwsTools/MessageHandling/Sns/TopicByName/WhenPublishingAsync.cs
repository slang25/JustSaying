using Amazon.SimpleNotificationService.Model;
using JustSaying.AwsTools.MessageHandling;
using JustSaying.Messaging;
using JustSaying.Messaging.Compression;
using JustSaying.TestingFramework;
using JustSaying.UnitTests.Messaging.Channels.TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace JustSaying.UnitTests.AwsTools.MessageHandling.Sns.TopicByName;

public class WhenPublishingAsync : WhenPublishingTestBase
{
    private const string Message = "the_message_in_json";
    private const string MessageAttributeKey = "StringAttribute";
    private const string MessageAttributeValue = "StringValue";
    private const string MessageAttributeDataType = "String";
    private const string TopicArn = "topicarn";

    private protected override Task<SnsMessagePublisher> CreateSystemUnderTestAsync()
    {
        var messageConverter = CreateConverter(new FakeBodySerializer(Message));
        var topic = new SnsMessagePublisher(TopicArn, Sns, messageConverter, NullLoggerFactory.Instance, null, null);
        return Task.FromResult(topic);
    }

    protected override void Given()
    {
        Sns.FindTopicAsync("TopicName")
            .Returns(new Topic { TopicArn = TopicArn });
    }

    protected override async Task WhenAsync()
    {
        var metadata = new PublishMetadata()
            .AddMessageAttribute(MessageAttributeKey, MessageAttributeValue);

        await SystemUnderTest.PublishAsync(new SimpleMessage(), metadata);
    }

    [Test]
    public async Task MessageIsPublishedToSnsTopic()
    {
        Sns.Received().PublishAsync(Arg.Is<PublishRequest>(x => B(x)));
    }

    private static bool B(PublishRequest x)
    {
        return x.Message.Equals(Message, StringComparison.OrdinalIgnoreCase);
    }


    [Test]
    public async Task MessageSubjectIsObjectType()
    {
        Sns.Received().PublishAsync(Arg.Is<PublishRequest>(x => x.Subject == typeof(SimpleMessage).Name));
    }

    [Test]
    public async Task MessageIsPublishedToCorrectLocation()
    {
        Sns.Received().PublishAsync(Arg.Is<PublishRequest>(x => x.TopicArn == TopicArn));
    }

    [Test]
    public async Task MessageAttributeKeyIsPublished()
    {
        Sns.Received().PublishAsync(Arg.Is<PublishRequest>(x =>
            x.MessageAttributes != null &&
            x.MessageAttributes.ContainsKey(MessageAttributeKey)
        ));
    }

    [Test]
    public async Task MessageAttributeValueIsPublished()
    {
        Sns.Received().PublishAsync(Arg.Is<PublishRequest>(x => x.MessageAttributes.Single().Value.StringValue == MessageAttributeValue));
    }

    [Test]
    public async Task MessageAttributeDataTypeIsPublished()
    {
        Sns.Received().PublishAsync(Arg.Is<PublishRequest>(x => x.MessageAttributes.Single().Value.DataType == MessageAttributeDataType));
    }
}
