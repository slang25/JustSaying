using JustSaying.Messaging.Channels.Receive;
using JustSaying.Messaging.Channels.SubscriptionGroups;
using JustSaying.TestingFramework;
using JustSaying.UnitTests.Messaging.Channels.TestHelpers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace JustSaying.UnitTests.JustSayingBus;

public abstract class GivenAServiceBus : IAsyncLifetime
{
    protected IMessagingConfig Config;
    protected IMessageReceivePauseSignal MessageReceivePauseSignal;
    protected TrackingLoggingMonitor Monitor;
    protected ILoggerFactory LoggerFactory;
    private bool _recordThrownExceptions;

    public ITestOutputHelper OutputHelper { get; private set; }
    protected Exception ThrownException { get; private set; }

    protected JustSaying.JustSayingBus SystemUnderTest { get; private set; }

    protected static readonly TimeSpan TimeoutPeriod = TimeSpan.FromSeconds(1);


    public GivenAServiceBus(ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;
        LoggerFactory = outputHelper.ToLoggerFactory();
    }

    public virtual async Task InitializeAsync()
    {
        Given();

        try
        {
            SystemUnderTest = CreateSystemUnderTest();
            await WhenAsync().ConfigureAwait(false);
        }
        catch (Exception ex) when (_recordThrownExceptions)
        {
            ThrownException = ex;
        }
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected virtual void Given()
    {
        Config = Substitute.For<IMessagingConfig>();
        MessageReceivePauseSignal = Substitute.For<IMessageReceivePauseSignal>();
        Monitor = new TrackingLoggingMonitor(LoggerFactory.CreateLogger<TrackingLoggingMonitor>());
    }

    protected abstract Task WhenAsync();

    private JustSaying.JustSayingBus CreateSystemUnderTest()
    {
        var serializerRegister = new FakeSerializationRegister();
        var bus = new JustSaying.JustSayingBus(Config,
            serializerRegister,
            MessageReceivePauseSignal,
            LoggerFactory,
            Monitor);

        bus.SetGroupSettings(new SubscriptionGroupSettingsBuilder()
                .WithDefaultConcurrencyLimit(8),
            new Dictionary<string, SubscriptionGroupConfigBuilder>());

        return bus;
    }

    public void RecordAnyExceptionsThrown()
    {
        _recordThrownExceptions = true;
    }
}
