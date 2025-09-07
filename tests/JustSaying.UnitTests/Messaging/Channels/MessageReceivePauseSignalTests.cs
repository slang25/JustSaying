using JustSaying.Messaging.Channels.Receive;

namespace JustSaying.UnitTests.Messaging.Channels;

public class MessageReceivePauseSignalTests
{
    [Test]
    public async Task WhenInitialized_ReturnsIsPausedFalse()
    {
        var messageReceivePauseSignal = new MessageReceivePauseSignal();

        var result = messageReceivePauseSignal.IsPaused;

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task WhenPaused_ReturnsIsPaused()
    {
        var messageReceivePauseSignal = new MessageReceivePauseSignal();

        messageReceivePauseSignal.Pause();

        var result = messageReceivePauseSignal.IsPaused;

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task WhenStarted_ReturnsIsPausedFalse()
    {
        var messageReceivePauseSignal = new MessageReceivePauseSignal();

        messageReceivePauseSignal.Resume();

        var result = messageReceivePauseSignal.IsPaused;

        await Assert.That(result).IsFalse();
    }
}
