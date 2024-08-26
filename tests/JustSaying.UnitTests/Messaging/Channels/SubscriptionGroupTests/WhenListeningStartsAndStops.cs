using Amazon.SQS.Model;
using JustSaying.Messaging.Channels.SubscriptionGroups;
using JustSaying.Messaging.MessageProcessingStrategies;
using Microsoft.Extensions.Logging;

namespace JustSaying.UnitTests.Messaging.Channels.SubscriptionGroupTests;

public class WhenListeningStartsAndStops(ITestOutputHelper testOutputHelper) : BaseSubscriptionGroupTests(testOutputHelper)
{
    private const string AttributeMessageContentsRunning = """{ "Subject": "SimpleMessage", "Message": "Message Contents Running" }""";
    private const string AttributeMessageContentsAfterStop = """{ "Subject": "SimpleMessage", "Message": "Message Contents After Stop" }""";

    private int _expectedMaxMessageCount;
    private bool _running;
    private FakeSqsQueue _queue;

    protected override void Given()
    {
        // we expect to get max 10 messages per batch
        _expectedMaxMessageCount = MessageDefaults.MaxAmazonMessageCap;

        Logger.LogInformation("Expected max message count is {MaxMessageCount}", _expectedMaxMessageCount);

        var response1 = new Message { Body = AttributeMessageContentsRunning };
        var response2 = new Message { Body = AttributeMessageContentsAfterStop } ;
        IEnumerable<Message> GetMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_running) yield return response1;
                else yield return response2;
            }
        }

        var sqsSource = CreateSuccessfulTestQueue(Guid.NewGuid().ToString(), ct => Task.FromResult(GetMessages(ct)));
        _queue = sqsSource.SqsQueue as FakeSqsQueue;

        Queues.Add(sqsSource);
    }

    protected override async Task WhenAsync()
    {
        _running = true;

        await base.WhenAsync();

        _running = false;
    }

    [Fact]
    public void MessagesAreReceived()
    {
        _queue.ReceiveMessageRequests.ShouldNotBeEmpty();
    }

    [Fact]
    public void TheMaxMessageAllowanceIsGrabbed()
    {
        _queue.ReceiveMessageRequests.ShouldAllBe(req => req.MaxNumOfMessages == _expectedMaxMessageCount);
    }

    [Fact]
    public void MessageIsProcessed()
    {
        // TODO
        throw new NotImplementedException();
    }
}
