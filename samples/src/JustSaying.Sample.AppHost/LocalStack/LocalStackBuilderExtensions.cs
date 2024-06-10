namespace JustSaying.Sample.AppHost.LocalStack;

public static class LocalStackBuilderExtensions
{
    public static IResourceBuilder<LocalStackResource> AddLocalStack(this IDistributedApplicationBuilder builder, string name)
    {
        var localstack = new LocalStackResource(name);

        return builder.AddResource(localstack)
            .WithHttpEndpoint(port: 4566, targetPort: 4566, name: LocalStackResource.PrimaryEndpointName)
            .WithImage(LocalStackContainerImageTags.Image, LocalStackContainerImageTags.Tag)
            .WithImageRegistry(LocalStackContainerImageTags.Registry)
            .WithEnvironment("SERVICES", "sqs,sns")
            .WithEnvironment("OVERRIDE_IN_DOCKER", "true")
            .WithEnvironment("EAGER_SERVICE_LOADING", "1")
            .WithEnvironment("LEGACY_DOCKER_CLIENT", "1")
            .WithEnvironment("DEBUG", "1");
    }
}
