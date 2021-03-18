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
                Arn
            });
        }

        public string QueueName { get; internal set; }
        public string RegionSystemName { get; internal set; }
        public Uri Uri { get; internal set; }
        public string Arn { get; internal set; }
        public IAmazonSQS Client { get; internal set; }
    }
}
