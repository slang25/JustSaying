using System.Diagnostics;
using System.Text.Json;
using Amazon.SQS.Model;
using AWS.Messaging;
using AWS.Messaging.Configuration;
using AWS.Messaging.Serialization;
using JustSaying.Messaging.MessageSerialization;
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

public class JustSayingAdapterSerializer : IEnvelopeSerializer
{
    private readonly SystemTextJsonSerializer _serializer = new();
    private readonly MessageSerializationRegister _messageSerializationRegister = new(new NonGenericMessageSubjectProvider(), new SystemTextJsonSerializationFactory());

    public JustSayingAdapterSerializer()
    {
        _messageSerializationRegister.AddSerializer<OrderReadyEvent>();
    }

    public ValueTask<string> SerializeAsync<T>(MessageEnvelope<T> envelope)
    {
        throw new NotImplementedException();
    }

    public ValueTask<MessageEnvelope<T>> CreateEnvelopeAsync<T>(T message)
    {
        // TODO
        throw new NotImplementedException();
    }

    public ValueTask<ConvertToEnvelopeResult> ConvertToEnvelopeAsync(Message sqsMessage)
    {
        try
        {
            var messageWithAttributes = _messageSerializationRegister.DeserializeMessage(sqsMessage.Body);
            // TODO double read of message here
            var subject = _serializer.GetMessageSubject(sqsMessage.Body);
            var subscriberMapping = new SubscriberMapping(typeof(OrderPlacedEventHandler), messageWithAttributes.Message.GetType(), subject);
            var message = messageWithAttributes.Message;
            var messageType = message.GetType();
            var messageEnvelopeType = typeof(MessageEnvelope<>).MakeGenericType(messageType);

            if (Activator.CreateInstance(messageEnvelopeType) is not MessageEnvelope finalMessageEnvelope)
            {
                throw new InvalidOperationException($"Failed to create a {nameof(MessageEnvelope)} of type '{messageEnvelopeType.FullName}'");
            }

            var source = "/justsaying";
            if (!string.IsNullOrEmpty(message.RaisingComponent))
            {
                source += $"/{message.RaisingComponent}";
            }

            finalMessageEnvelope.Id = message.Id.ToString();
            finalMessageEnvelope.Source = new Uri(source, UriKind.Relative);
            finalMessageEnvelope.Version = "1.0.0";
            finalMessageEnvelope.MessageTypeIdentifier = subject;
            finalMessageEnvelope.TimeStamp = message.TimeStamp.ToUniversalTime();
            finalMessageEnvelope.Metadata = new Dictionary<string, JsonElement>();
            finalMessageEnvelope.SQSMetadata = new SQSMetadata
            {
                //MessageAttributes = messageWithAttributes.MessageAttributes
            };
            finalMessageEnvelope.SNSMetadata = default;
            finalMessageEnvelope.EventBridgeMetadata = default;
            // Set finalMessageEnvelope.Message using reflection
            var messageProperty = messageEnvelopeType.GetProperty("Message");
            Debug.Assert(messageProperty != null, nameof(messageProperty) + " != null");
            messageProperty.SetValue(finalMessageEnvelope, message);

            var result = new ConvertToEnvelopeResult(finalMessageEnvelope, subscriberMapping);

            return ValueTask.FromResult(result);
        }
        catch (Exception ex)
        {
            throw new FailedToCreateMessageEnvelopeException($"Failed to create {nameof(MessageEnvelope)}", ex);
        }
    }
}
