using System.Security.Cryptography;
using AWS.Messaging;
using JustSaying.Messaging;
using JustSaying.Messaging.MessageHandling;
using JustSaying.Sample.Restaurant.Models;
using Microsoft.Extensions.Logging;
using IMessagePublisher = JustSaying.Messaging.IMessagePublisher;

namespace JustSaying.Sample.Restaurant.KitchenConsole.Handlers;

public class OrderOnItsWayEventHandler(IMessagePublisher publisher, ILogger<OrderOnItsWayEventHandler> logger) : IHandlerAsync<OrderOnItsWayEvent>, IMessageHandler<OrderOnItsWayEvent>
{
    public async Task<bool> Handle(OrderOnItsWayEvent message)
    {
        await Task.Delay(RandomNumberGenerator.GetInt32(50, 100));

        var orderDeliveredEvent = new OrderDeliveredEvent()
        {
            OrderId = message.OrderId
        };

        logger.LogInformation("Order {OrderId} is on its way!", message.OrderId);

        await publisher.PublishAsync(orderDeliveredEvent);

        return true;
    }

    public async Task<MessageProcessStatus> HandleAsync(MessageEnvelope<OrderOnItsWayEvent> messageEnvelope, CancellationToken token = default)
    {
        var result = await Handle(messageEnvelope.Message);
        return result ? MessageProcessStatus.Success() : MessageProcessStatus.Failed();
    }
}
