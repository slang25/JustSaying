using JustSaying.Fluent;

namespace JustSaying.UnitTests.Fluent;

public class TopicAddressTests
{
    [Test]
    public async Task ParsingEmptyArnThrows()
    {
        Assert.Throws<ArgumentException>("topicArn", () => TopicAddress.FromArn(""));
    }

    [Test]
    public async Task ParsingNullArnThrows()
    {
        Assert.Throws<ArgumentException>("topicArn", () => TopicAddress.FromArn(null));
    }

    [Test]
    public async Task ValidArnCanBeParsed()
    {
        var ta = TopicAddress.FromArn("arn:aws:sns:eu-west-1:111122223333:topic1");

        await Assert.That(ta.TopicArn).IsEqualTo("arn:aws:sns:eu-west-1:111122223333:topic1");
    }

    [Test]
    public async Task ArnForWrongServiceThrows()
    {
        Assert.Throws<ArgumentException>("topicArn", () => TopicAddress.FromArn("arn:aws:sqs:eu-west-1:111122223333:queue1"));
    }
}