using JustSaying.Messaging.Channels.SubscriptionGroups;
using JustSaying.TestingFramework;

namespace JustSaying.UnitTests.Messaging.Channels.SubscriptionGroupTests;

public class WhenMessageHandlingFails(ITestOutputHelper testOutputHelper) : BaseSubscriptionGroupTests(testOutputHelper)
{
    private FakeSqsQueue _queue;

    protected override void Given()
    {
        var sqsSource = CreateSuccessfulTestQueue(Guid.NewGuid().ToString(), new TestMessage());
        _queue = sqsSource.SqsQueue as FakeSqsQueue;

        Queues.Add(sqsSource);
        Handler.ShouldSucceed = false;
    }

    [Test]
    public async Task MessageHandlerWasCalled()
    {
        Handler.ReceivedMessages.ShouldNotBeEmpty();
    }

    [Test]
    public async Task FailedMessageIsNotRemovedFromQueue()
    {
        _queue.DeleteMessageRequests.ShouldBeEmpty();
    }

    [Test]
    public async Task ExceptionIsNotLoggedToMonitor()
    {
        Monitor.HandledExceptions.ShouldBeEmpty();
    }
}
