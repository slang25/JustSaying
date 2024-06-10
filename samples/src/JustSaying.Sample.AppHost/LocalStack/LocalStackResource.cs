namespace JustSaying.Sample.AppHost.LocalStack;

public class LocalStackResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "http";

    private EndpointReference? _primaryEndpoint;

    public EndpointReference PrimaryEndpoint => _primaryEndpoint ??= new(this, PrimaryEndpointName);

    public ReferenceExpression ConnectionStringExpression => ReferenceExpression.Create($"{PrimaryEndpoint.Property(EndpointProperty.Url)}");
}
