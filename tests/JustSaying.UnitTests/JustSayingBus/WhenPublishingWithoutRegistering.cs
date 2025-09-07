using JustSaying.Messaging;
using JustSaying.Models;
using NSubstitute;

namespace JustSaying.UnitTests.JustSayingBus;

public class WhenPublishingWithoutRegistering(ITestOutputHelper outputHelper) : GivenAServiceBus(outputHelper)
{
    protected override void Given()
    {
        base.Given();
        RecordAnyExceptionsThrown();
    }

    protected override async Task WhenAsync()
    {
        await SystemUnderTest.PublishAsync(Substitute.For<Message>());
    }

    [Test]
    public async Task InvalidOperationIsThrown()
    {
        ThrownException.ShouldBeAssignableTo<InvalidOperationException>();
    }
}