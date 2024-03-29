using AWS.Messaging.Serialization;
using JustSaying.Sample.Restaurant.KitchenConsole;
using JustSaying.Sample.Restaurant.KitchenConsole.Handlers;
using JustSaying.Sample.Restaurant.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

const string appName = "KitchenConsole";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Debug()
    .Enrich.WithProperty("AppName", appName)
    .CreateLogger();

Console.Title = appName;

try
{
    await Run();
}
catch (Exception e)
{
    Log.Fatal(e, "Error occurred during startup: {Message}", e.Message);
}
finally
{
    Log.CloseAndFlush();
}

static async Task Run()
{
    await new HostBuilder()
        .ConfigureAppConfiguration((hostContext, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: false);
            config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);
            config.AddEnvironmentVariables();
        })
        .UseSerilog()
        .ConfigureServices((hostContext, services) =>
        {
            var configuration = hostContext.Configuration;

            services.AddDefaultAWSOptions(configuration.GetAWSOptions());
            services.AddSingleton<IEnvelopeSerializer, JustSayingAdapterSerializer>();
            services.AddAWSMessageBus(builder =>
            {
                // Register that you'll publish messages of type ChatMessage to an existing queue
                builder.AddSQSPoller("http://localhost.localstack.cloud:4566/000000000000/orderreadyevent",
                    o =>
                    {
                        // needs better docs, also use timespans not just ints
                        //o.WaitTimeSeconds = 10;

                        // Should I be able to set an envelope serializer here?
                    });

                builder.AddMessageHandler<OrderPlacedEventHandler, OrderReadyEvent>();
                builder.AddMessageHandler<OrderOnItsWayEventHandler, OrderOnItsWayEvent>();
            });
        })
        .UseConsoleLifetime()
        .Build()
        .RunAsync();
}
