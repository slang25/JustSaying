using JustSaying.Fluent;

namespace JustSaying.UnitTests.Fluent;

public class QueueAddressTests
{
    [Test]
    public async Task ParsingEmptyArnThrows()
    {
        Assert.Throws<ArgumentException>("queueArn",() => QueueAddress.FromArn(""));
    }

    [Test]
    public async Task ParsingNullArnThrows()
    {
        Assert.Throws<ArgumentException>("queueArn", () => QueueAddress.FromArn(null));
    }

    [Test]
    public async Task ValidArnCanBeParsed()
    {
        var qa = QueueAddress.FromArn("arn:aws:sqs:eu-west-1:111122223333:queue1");

        await Assert.That(qa.QueueUrl.AbsoluteUri).IsEqualTo("https://sqs.eu-west-1.amazonaws.com/111122223333/queue1");
        await Assert.That(qa.RegionName).IsEqualTo("eu-west-1");
    }

    [Test]
    public async Task ArnForWrongServiceThrows()
    {
        Assert.Throws<ArgumentException>("queueArn", () => QueueAddress.FromArn("arn:aws:sns:eu-west-1:111122223333:queue1"));
    }

    [Test]
    public async Task ValidUrlCanBeParsed()
    {
        var qa = QueueAddress.FromUrl("https://sqs.eu-west-1.amazonaws.com/111122223333/queue1");

        await Assert.That(qa.QueueUrl.AbsoluteUri).IsEqualTo("https://sqs.eu-west-1.amazonaws.com/111122223333/queue1");
        await Assert.That(qa.RegionName).IsEqualTo("eu-west-1");
    }

    [Test]
    public async Task UppercaseUrlCanBeParsed()
    {
        var qa = QueueAddress.FromUrl("HTTPS://SQS.EU-WEST-1.AMAZONAWS.COM/111122223333/Queue1");

        // Queue name is case-sensitive.
        await Assert.That(qa.QueueUrl.AbsoluteUri).IsEqualTo("https://sqs.eu-west-1.amazonaws.com/111122223333/Queue1");
        await Assert.That(qa.RegionName).IsEqualTo("eu-west-1");
    }

    [Test]
    public async Task LocalStackUrlWithoutRegionHashUnknownRegion()
    {
        var qa = QueueAddress.FromUrl("http://localhost:4576/111122223333/queue1");

        await Assert.That(qa.RegionName).IsEqualTo("unknown");
    }

    [Test]
    public async Task LocalStackUrlWithRegionCanBeParsed()
    {
        var qa = QueueAddress.FromUrl("http://localhost:4576/111122223333/queue1","us-east-1");

        await Assert.That(qa.QueueUrl.AbsoluteUri).IsEqualTo("http://localhost:4576/111122223333/queue1");
        await Assert.That(qa.RegionName).IsEqualTo("us-east-1");
    }

    [Test]
    public async Task EmptyUrlThrows()
    {
        Assert.Throws<ArgumentException>("queueUrl", () => QueueAddress.FromUrl(""));
    }
}