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
        private readonly ParsedSqsQueueUrl _queueUrlUrl;

        public AttachedQueueSubscriptionBuilder(string queueUrl)
        {
            _queueUrlUrl = ParsedSqsQueueUrl.Parse(queueUrl);
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

            attachedQueueConfig.SubscriptionGroupName ??= _queueUrlUrl.QueueName;
            attachedQueueConfig.MiddlewareConfiguration = attachedQueueConfig.MiddlewareConfiguration;
            attachedQueueConfig.Validate();

            IAmazonSQS sqsClient = serviceResolver
                .ResolveService<IAwsClientFactory>()
                .GetSqsClient(RegionEndpoint.GetBySystemName(_queueUrlUrl.Region));

            var queue = new AttachedQueue
            {
                Uri = _queueUrlUrl.QueueUri,
                QueueName = _queueUrlUrl.QueueName,
                RegionSystemName = _queueUrlUrl.Region,
                Client = sqsClient
            };

            bus.AddQueue(attachedQueueConfig.SubscriptionGroupName, queue);

            logger.LogInformation(
                "Added SQS queue subscription for '{QueueName}'.",
                _queueUrlUrl.QueueName);

            var resolutionContext = new HandlerResolutionContext(_queueUrlUrl.QueueName);
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

            bus.AddMessageMiddleware<T>(_queueUrlUrl.QueueName, handlerMiddleware);

            logger.LogInformation(
                "Added a message handler for message type for '{MessageType}' on queue '{QueueName}'.",
                typeof(T),
                _queueUrlUrl.QueueName);
        }
    }
}
