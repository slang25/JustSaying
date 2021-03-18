using System;
using JustSaying.Messaging.Middleware;

namespace JustSaying.AwsTools.QueueCreation
{
    public sealed class AttachedQueueConfig
    {
        public string SubscriptionGroupName { get; set; }
        public Action<HandlerMiddlewareBuilder> MiddlewareConfiguration { get; set; }

        public void Validate()
        {
        }
    }
}