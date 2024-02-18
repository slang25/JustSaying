using System.Security.Cryptography;
using AWS.Messaging;
using JustSaying.Messaging;
using JustSaying.Messaging.MessageHandling;
using JustSaying.Sample.Restaurant.Models;
using Microsoft.Extensions.Logging;
using IMessagePublisher = JustSaying.Messaging.IMessagePublisher;

namespace JustSaying.Sample.Restaurant.KitchenConsole.Handlers;

/// <summary>
/// Handles messages of type OrderPlacedEvent
/// Takes a dependency on IMessagePublisher so that further messages can be published
/// </summary>
public class OrderPlacedEventHandler(ILogger<OrderPlacedEventHandler> logger) : IHandlerAsync<OrderReadyEvent>, IMessageHandler<OrderReadyEvent>
{
    public async Task<bool> Handle(OrderReadyEvent message)
    {
        // Returning true would indicate:
        //   The message was handled successfully
        //   The message can be removed from the queue.
        // Returning false would indicate:
        //   The message was not handled successfully
        //   The message handling should be retried (configured by default)
        //   The message should be moved to the error queue if all retries fail

        try
        {
            logger.LogInformation("Order {orderId} received", message.OrderId);

            // This is where you would actually handle the order placement
            // Intentionally left empty for the sake of this being a sample application

            logger.LogInformation("Order {OrderId} ready", message.OrderId);

            await Task.Delay(RandomNumberGenerator.GetInt32(50, 100));

            var orderReadyEvent = new OrderReadyEvent
            {
                OrderId = message.OrderId
            };

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to handle message for {orderId}", message.OrderId);
            return false;
        }
    }

    public async Task<MessageProcessStatus> HandleAsync(MessageEnvelope<OrderReadyEvent> messageEnvelope, CancellationToken token = new CancellationToken())
    {
        var result = await Handle(messageEnvelope.Message);
        return result ? MessageProcessStatus.Success() : MessageProcessStatus.Failed();
    }
}
