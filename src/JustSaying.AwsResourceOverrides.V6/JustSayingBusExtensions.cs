namespace JustSaying.AwsResourceOverrides;

public static class JustSayingBusExtensions
{
    public static IMayWantOptionalSettings WithNamingOverrides(
        this IMayWantNamingStrategy bus,
        Dictionary<Type, string> queueNameOverrides,
        Dictionary<Type, string> topicNameOverrides)
    {
        var defaultNamingStrategy = ((JustSayingFluently)bus).GetNamingStrategy();
        return bus.WithNamingStrategy(() => new OverrideNamingStrategy(defaultNamingStrategy, queueNameOverrides, topicNameOverrides));
    }
}
