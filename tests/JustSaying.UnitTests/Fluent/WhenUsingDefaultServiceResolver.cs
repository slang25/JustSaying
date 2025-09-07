using JustSaying.AwsTools;
using JustSaying.Fluent;
using JustSaying.Messaging.MessageSerialization;
using JustSaying.Naming;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace JustSaying.UnitTests.Fluent;

public class WhenUsingDefaultServiceResolver
{
    private readonly DefaultServiceResolver _sut = new();

    [Test]
    public async Task ShouldResolveILoggerFactoryToNullLoggerFactory()
    {
        _sut.ResolveService<ILoggerFactory>().ShouldBeOfType<NullLoggerFactory>();
    }

    [Test]
    public async Task ShouldResolveIAwsClientFactoryProxyToAwsClientFactoryProxy()
    {
        _sut.ResolveService<IAwsClientFactoryProxy>().ShouldBeOfType<AwsClientFactoryProxy>();
    }

    [Test]
    public async Task ShouldResolveIHandlerResolverAsNull()
    {
        _sut.ResolveOptionalService<IHandlerResolver>().ShouldBeNull();
    }

    [Test]
    public async Task ShouldResolveIMessagingConfigToMessagingConfig()
    {
        _sut.ResolveService<IMessagingConfig>().ShouldBeOfType<MessagingConfig>();
    }

    [Test]
    public async Task ShouldResolveIMessageSerializationFactoryToNewtonsoftSerializationFactory()
    {
        _sut.ResolveService<IMessageBodySerializationFactory>().ShouldBeOfType<NewtonsoftSerializationFactory>();
    }

    [Test]
    public async Task ShouldResolveIMessageSubjectProviderToNonGenericMessageSubjectProvider()
    {
        _sut.ResolveService<IMessageSubjectProvider>().ShouldBeOfType<NonGenericMessageSubjectProvider>();
    }

    [Test]
    public async Task ShouldResolveITopicNamingConventionToDefaultNamingConvention()
    {
        _sut.ResolveService<ITopicNamingConvention>().ShouldBeOfType<DefaultNamingConventions>();
    }

    [Test]
    public async Task ShouldResolveIQueueNamingConventionToDefaultNamingConvention()
    {
        _sut.ResolveService<IQueueNamingConvention>().ShouldBeOfType<DefaultNamingConventions>();
    }
}
