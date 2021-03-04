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

    internal sealed class ParsedSqsQueueUrl
    {
        private ParsedSqsQueueUrl(Uri queueUri, string region, string accountId, string queueName)
        {
            QueueUri = queueUri;
            Region = region;
            AccountId = accountId;
            QueueName = queueName;
        }

        public static ParsedSqsQueueUrl Parse(string queueUrl)
        {
            if (queueUrl == null) throw new ArgumentNullException(nameof(queueUrl));
            var queueUri = new Uri(queueUrl);
            var hostParts = queueUri.Host.Split('.');
            var pathParts = queueUri.PathAndQuery.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (hostParts.Length != 4) throw new Exception(); // TODO what about localstack etc?
            if (pathParts.Length != 2) throw new Exception();

            var region = hostParts[1];
            var accountId = pathParts[0];
            var queueName = pathParts[1];
            return new ParsedSqsQueueUrl(queueUri, region, accountId, queueName);
        }

        public Uri QueueUri { get; }
        public string Region { get; }
        public string AccountId { get; }
        public string QueueName { get; }
    }
}
