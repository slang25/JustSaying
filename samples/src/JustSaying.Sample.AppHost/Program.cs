var builder = DistributedApplication.CreateBuilder(args);

var localStack = builder.AddContainer("localstack", "localstack/localstack", "stable")
    .WithHttpEndpoint(targetPort: 4566)
    .WithEnvironment("SERVICES", "sqs,sns")
    .WithEnvironment("DEBUG", "1");

builder
    .AddProject<Projects.JustSaying_Sample_Restaurant_KitchenConsole>("KitchenConsole")
    .WithEnvironment("AWSServiceUrl", localStack.Resource.GetEndpoint("http"));

builder
    .AddProject<Projects.JustSaying_Sample_Restaurant_OrderingApi>("OrderingApi")
    .WithEnvironment("AWSServiceUrl", localStack.Resource.GetEndpoint("http"));

builder.Build().Run();
