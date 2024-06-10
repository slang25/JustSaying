using JustSaying.Sample.AppHost.LocalStack;

var builder = DistributedApplication.CreateBuilder(args);
var localStack = builder.AddLocalStack("localstack");

builder
    .AddProject<Projects.JustSaying_Sample_Restaurant_KitchenConsole>("kitchen-console")
    .WithReference(localStack);

builder
    .AddProject<Projects.JustSaying_Sample_Restaurant_OrderingApi>("ordering-api",
        launchProfileName: "JustSaying.Sample.Restaurant.OrderingApi")
    .WithReference(localStack);

builder.Build().Run();
