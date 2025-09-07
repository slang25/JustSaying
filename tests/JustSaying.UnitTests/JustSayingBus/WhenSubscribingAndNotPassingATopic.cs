namespace JustSaying.UnitTests.JustSayingBus;

public class WhenSubscribingAndNotPassingATopic(ITestOutputHelper outputHelper) : GivenAServiceBus(outputHelper)
{
    protected override void Given()
    {
        base.Given();
        RecordAnyExceptionsThrown();
    }

    protected override Task WhenAsync()
    {
        SystemUnderTest.AddQueue("test", null);
        return Task.CompletedTask;
    }

    [Test]
    public async Task ArgExceptionThrown()
    {
        ((ArgumentException)ThrownException).ParamName.ShouldBe("queue");
    }
}