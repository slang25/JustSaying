using System;
using Amazon.SQS;
using JustSaying.Messaging.Interrogation;

namespace JustSaying.AwsTools.MessageHandling
{
    internal sealed class ExistingQueue : ISqsQueue
    {
        public ExistingQueue(string queueName, string regionSystemName, string accountId, IAmazonSQS client)
        {
            QueueName = queueName;
            RegionSystemName = regionSystemName;
            Client = client;
            Uri = new Uri($"https://sqs.{RegionSystemName}.amazonaws.com/{accountId}/{QueueName}");
        }

        public InterrogationResult Interrogate()
        {
            return new(new
            {
                Uri,
                QueueName,
                RegionSystemName
            });
        }

        public string QueueName { get; }
        public string RegionSystemName { get; }
        public Uri Uri { get; }
        public string Arn => string.Empty;
        public IAmazonSQS Client { get; }
    }
}
