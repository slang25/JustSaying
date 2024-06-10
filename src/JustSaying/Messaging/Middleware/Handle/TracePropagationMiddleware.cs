using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace JustSaying.Messaging.Middleware;

public class TracePropagationMiddleware : MiddlewareBase<HandleMessageContext, bool>
{
    private static readonly ActivitySource ActivitySource = new("JustSaying", "7.0.0");

    protected override async Task<bool> RunInnerAsync(HandleMessageContext context, Func<CancellationToken, Task<bool>> func, CancellationToken stoppingToken)
    {
        var propagatedContext = Propagators.DefaultTextMapPropagator.Extract(default, context, ExtractTraceContextFromEnvelope);
        Baggage.Current = propagatedContext.Baggage;
        //using var activity = ActivitySource.StartActivity("Processing message", ActivityKind.Consumer, parentContext: propagatedContext.ActivityContext);
        // created linked activity
        return await func(stoppingToken).ConfigureAwait(false);
    }

    private static IEnumerable<string> ExtractTraceContextFromEnvelope(HandleMessageContext context, string key)
    {
        var v = context.MessageAttributes.Get(key);
        if (v is null) return [];
        return [v.StringValue];
    }
}
