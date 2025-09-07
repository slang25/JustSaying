using JustSaying.Fluent;
using JustSaying.Naming;
using JustSaying.TestingFramework;

namespace JustSaying.UnitTests.Fluent;

public class AccountAddressProviderTests
{
    [Test]
    public async Task CanGetAccountQueueByName()
    {
        var sut = new AccountAddressProvider("123456789012", "eu-west-1");
        var address = sut.GetQueueUri("queue1");

        await Assert.That(address).IsEqualTo(new Uri(" https://sqs.eu-west-1.amazonaws.com/123456789012/queue1"));
    }

    [Test]
    public async Task CanGetAccountTopicByName()
    {
        var sut = new AccountAddressProvider("123456789012", "eu-west-1");
        var address = sut.GetTopicArn("topic1");

        await Assert.That(address).IsEqualTo("arn:aws:sns:eu-west-1:123456789012:topic1");
    }

    [Test]
    public async Task CanGetAccountQueueByDefaultConvention()
    {
        var sut = new AccountAddressProvider("123456789012", "eu-west-1");
        var address = sut.GetQueueUriByConvention<Order>();

        await Assert.That(address).IsEqualTo(new Uri(" https://sqs.eu-west-1.amazonaws.com/123456789012/order"));
    }

    [Test]
    public async Task CanGetAccountTopicByDefaultConvention()
    {
        var sut = new AccountAddressProvider("123456789012", "eu-west-1");
        var address = sut.GetTopicArnByConvention<Order>();

        await Assert.That(address).IsEqualTo("arn:aws:sns:eu-west-1:123456789012:order");
    }

    [Test]
    public async Task CanGetAccountQueueByCustomConvention()
    {
        var convention = new ManualNamingConvention("adhoc-queue-name", null);
        var sut = new AccountAddressProvider("123456789012", "eu-west-1", convention, null);
        var address = sut.GetQueueUriByConvention<Order>();

        await Assert.That(address).IsEqualTo(new Uri(" https://sqs.eu-west-1.amazonaws.com/123456789012/adhoc-queue-name"));
    }

    [Test]
    public async Task CanGetAccountTopicByCustomConvention()
    {
        var convention = new ManualNamingConvention(null, "adhoc-topic-name");
        var sut = new AccountAddressProvider("123456789012", "eu-west-1", null, convention);
        var address = sut.GetTopicArnByConvention<Order>();

        await Assert.That(address).IsEqualTo("arn:aws:sns:eu-west-1:123456789012:adhoc-topic-name");
    }

    private class ManualNamingConvention(string queueName, string topicName) : IQueueNamingConvention, ITopicNamingConvention
    {
        private readonly string _queueName = queueName;
        private readonly string _topicName = topicName;

        public string QueueName<T>() => _queueName;
        public string TopicName<T>() => _topicName;
    }
}