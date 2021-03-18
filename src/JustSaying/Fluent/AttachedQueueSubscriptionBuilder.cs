using System;
using Amazon;
using Amazon.SQS;
using JustSaying.AwsTools;
using JustSaying.AwsTools.MessageHandling;
using JustSaying.AwsTools.QueueCreation;
using JustSaying.Messaging.Middleware;
using JustSaying.Models;
using Microsoft.Extensions.Logging;

namespace JustSaying.Fluent
{
    public sealed class AttachedQueueSubscriptionBuilder<T> : ISubscriptionBuilder<T>
        where T : Message
    {
        private readonly Arn _queueArn;

        public AttachedQueueSubscriptionBuilder(string queueArn)
        {
            if (!Arn.TryParse(queueArn, out var arn)) throw new Exception("Oh noes!");
            _queueArn = arn;
        }

        private Action<AttachedQueueConfig> ConfigureReads { get; set; }

        public AttachedQueueSubscriptionBuilder<T> WithReadConfiguration(Action<AttachedQueueConfig> configure)
        {
            ConfigureReads = configure ?? throw new ArgumentNullException(nameof(configure));
            return this;
        }

        void ISubscriptionBuilder<T>.Configure(
            JustSayingBus bus,
            IHandlerResolver handlerResolver,
            IServiceResolver serviceResolver,
            IVerifyAmazonQueues creator,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<AttachedQueueSubscriptionBuilder<T>>();

            var attachedQueueConfig = new AttachedQueueConfig();

            ConfigureReads?.Invoke(attachedQueueConfig);

            attachedQueueConfig.SubscriptionGroupName ??= _queueArn.Resource;
            attachedQueueConfig.MiddlewareConfiguration = attachedQueueConfig.MiddlewareConfiguration;
            attachedQueueConfig.Validate();

            IAmazonSQS sqsClient = serviceResolver
                .ResolveService<IAwsClientFactory>()
                .GetSqsClient(RegionEndpoint.GetBySystemName(_queueArn.Region));

            var queue = new AttachedQueue
            {
                Arn = _queueArn.ToString(),
                Uri = ArnToQueueUrl(_queueArn),
                QueueName = _queueArn.Resource,
                RegionSystemName = _queueArn.Region,
                Client = sqsClient
            };

            bus.AddQueue(attachedQueueConfig.SubscriptionGroupName, queue);

            logger.LogInformation(
                "Added SQS queue subscription for '{QueueName}'.",
                _queueArn.Resource);

            var resolutionContext = new HandlerResolutionContext(_queueArn.Resource);
            var proposedHandler = handlerResolver.ResolveHandler<T>(resolutionContext);
            if (proposedHandler == null)
            {
                throw new HandlerNotRegisteredWithContainerException(
                    $"There is no handler for '{typeof(T)}' messages.");
            }

            var middlewareBuilder = new HandlerMiddlewareBuilder(handlerResolver, serviceResolver);

            var handlerMiddleware = middlewareBuilder
                .UseHandler<T>()
                .UseStopwatch(proposedHandler.GetType())
                .Configure(attachedQueueConfig.MiddlewareConfiguration)
                .Build();

            bus.AddMessageMiddleware<T>(_queueArn.Resource, handlerMiddleware);

            logger.LogInformation(
                "Added a message handler for message type for '{MessageType}' on queue '{QueueName}'.",
                typeof(T),
                _queueArn.Resource);
        }

        private static Uri ArnToQueueUrl(Arn queueArn)
        {
            var dnsSuffix = RegionEndpoint.GetBySystemName(queueArn.Region).PartitionDnsSuffix;
            return new Uri($"https://{queueArn.Service}.{queueArn.Region}.{dnsSuffix}/{queueArn.AccountId}/{queueArn.Resource}");
        }
    }
}
