using JustSaying.AwsTools.MessageHandling.Dispatch;
using JustSaying.TestingFramework;
using JustSaying.UnitTests.AwsTools.MessageHandling;
using HandleMessageMiddleware = JustSaying.Messaging.Middleware.MiddlewareBase<JustSaying.Messaging.Middleware.HandleMessageContext, bool>;

namespace JustSaying.UnitTests.Messaging.Middleware;

public class MiddlewareMapTests
{
    [Test]
    public async Task EmptyMapDoesNotContain()
    {
        var map = CreateMiddlewareMap();
        map.Contains("queue", typeof(SimpleMessage)).ShouldBeFalse();
    }

    [Test]
    public async Task EmptyMapReturnsNullMiddleware()
    {
        var map = CreateMiddlewareMap();

        var handler = map.Get("queue", typeof(SimpleMessage));

        handler.ShouldBeNull();
    }

    [Test]
    public async Task MiddlewareIsReturnedForMatchingType()
    {
        var map = CreateMiddlewareMap();

        var middleware = new DelegateMessageHandlingMiddleware<SimpleMessage>(m => Task.FromResult(true));
        map.Add<SimpleMessage>("queue",  middleware);

        var handler = map.Get("queue", typeof(SimpleMessage));

        handler.ShouldNotBeNull();
    }

    [Test]
    public async Task MiddlewareContainsKeyForMatchingTypeOnly()
    {
        var map = CreateMiddlewareMap();
        var middleware = new DelegateMessageHandlingMiddleware<SimpleMessage>(m => Task.FromResult(true));
        map.Add<SimpleMessage>("queue", middleware);

        map.Contains("queue", typeof(SimpleMessage)).ShouldBeTrue();
        map.Contains("queue", typeof(AnotherSimpleMessage)).ShouldBeFalse();
    }

    [Test]
    public async Task MiddlewareIsNotReturnedForNonMatchingType()
    {
        var map = CreateMiddlewareMap();
        var middleware = new DelegateMessageHandlingMiddleware<SimpleMessage>(m => Task.FromResult(true));
        map.Add<SimpleMessage>("queue", middleware);

        var handler = map.Get("queue", typeof(AnotherSimpleMessage));

        handler.ShouldBeNull();
    }

    [Test]
    public async Task MultipleMiddlewareForATypeAreNotSupported()
    {
        HandleMessageMiddleware fn1 = new DelegateMessageHandlingMiddleware<SimpleMessage>(m => Task.FromResult(true));
        HandleMessageMiddleware fn2 = new DelegateMessageHandlingMiddleware<SimpleMessage>(m => Task.FromResult(true));

        var map = CreateMiddlewareMap();

        map.Add<SimpleMessage>("queue", fn1);
        map.Add<SimpleMessage>("queue", fn2);

        // Last in wins
        map.Get("queue", typeof(SimpleMessage)).ShouldBe(fn2);
    }

    [Test]
    public async Task MultipleMiddlewareForATypeWithOtherHandlersAreNotSupported()
    {
        HandleMessageMiddleware fn1 = new DelegateMessageHandlingMiddleware<SimpleMessage>(m => Task.FromResult(true));
        HandleMessageMiddleware fn2 = new DelegateMessageHandlingMiddleware<SimpleMessage>(m => Task.FromResult(false));
        HandleMessageMiddleware fn3 = new DelegateMessageHandlingMiddleware<AnotherSimpleMessage>(m => Task.FromResult(true));

        var map = CreateMiddlewareMap();
        map.Add<SimpleMessage>("queue", fn1);
        map.Add<AnotherSimpleMessage>("queue", fn3);
        map.Add<SimpleMessage>("queue", fn2);

        // Last in wins
        map.Get("queue", typeof(SimpleMessage)).ShouldBe(fn2);
        map.Get("queue", typeof(AnotherSimpleMessage)).ShouldBe(fn3);
    }

    [Test]
    public async Task MiddlewareIsNotReturnedForAnotherQueue()
    {
        string queue1 = "queue1";
        string queue2 = "queue2";
        var map = CreateMiddlewareMap();

        map.Add<SimpleMessage>(queue1,
            new DelegateMessageHandlingMiddleware<SimpleMessage>(m => Task.FromResult(true)));

        var handler = map.Get(queue2, typeof(SimpleMessage));

        handler.ShouldBeNull();
    }

    [Test]
    public async Task MiddlewareContainsKeyForMatchingQueueOnly()
    {
        string queue1 = "queue1";
        string queue2 = "queue2";
        var map = CreateMiddlewareMap();

        map.Add<SimpleMessage>(queue1,
            new DelegateMessageHandlingMiddleware<SimpleMessage>(m => Task.FromResult(true)));

        map.Contains(queue1, typeof(SimpleMessage)).ShouldBeTrue();
        map.Contains(queue2, typeof(AnotherSimpleMessage)).ShouldBeFalse();
    }

    [Test]
    public async Task MiddlewareHandlerIsReturnedForQueue()
    {
        string queue1 = "queue1";
        string queue2 = "queue2";

        var map = CreateMiddlewareMap();
        HandleMessageMiddleware fn1 = new DelegateMessageHandlingMiddleware<SimpleMessage>(m => Task.FromResult(true));
        map.Add<SimpleMessage>(queue1,fn1);

        HandleMessageMiddleware fn2 = new DelegateMessageHandlingMiddleware<SimpleMessage>(m => Task.FromResult(true));
        map.Add<SimpleMessage>(queue2, fn2);

        var handler1 = map.Get(queue1, typeof(SimpleMessage));
        handler1.ShouldBe(fn1);

        var handler2 = map.Get(queue2, typeof(SimpleMessage));
        handler2.ShouldBe(fn2);
    }

    private static MiddlewareMap CreateMiddlewareMap()
    {
        return new MiddlewareMap();
    }
}