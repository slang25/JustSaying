using System;
using Amazon.SQS;
using JustSaying.Messaging.Interrogation;

namespace JustSaying.AwsTools.MessageHandling
{
    internal sealed class AttachedQueue : ISqsQueue
    {
        public InterrogationResult Interrogate()
        {
            return new InterrogationResult(new
            {
                QueueName,
                Region = RegionSystemName,
                Uri
            });
        }

        public string QueueName { get; internal set; }
        public string RegionSystemName { get; internal set; }
        public Uri Uri { get; internal set; }

        // TODO Perhaps Arn shouldn't exist on ISqsQueue, and we have a more specific interface that includes it
        public string Arn => string.Empty; // Only really used for creating Topic -> Queue subscription
        public IAmazonSQS Client { get; internal set; }
    }
}
