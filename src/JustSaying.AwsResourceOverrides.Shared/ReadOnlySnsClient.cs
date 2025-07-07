using System.Net;
using Amazon.Runtime;
using Amazon.Runtime.SharedInterfaces;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Endpoint = Amazon.Runtime.Endpoints.Endpoint;

namespace JustSaying.AwsResourceOverrides;

public sealed class ReadOnlySnsClient : IAmazonSimpleNotificationService
{
    public ReadOnlySnsClient(IAmazonSimpleNotificationService innerClient, List<string> readonlyTopicArns)
    {
        _innerClient = innerClient;
        _readonlyTopicArns = readonlyTopicArns;
    }

    async Task<CreateTopicResponse> IAmazonSimpleNotificationService.CreateTopicAsync(CreateTopicRequest request, CancellationToken cancellationToken)
    {
        var topicArn = _readonlyTopicArns.FirstOrDefault(x => x.EndsWith(':' + request.Name, StringComparison.OrdinalIgnoreCase));
        if (topicArn is not null)
        {
            bool topicExists;
            try
            {
                // The most efficient way to check for existence is to ask for
                // the attributes. If the topic doesn't exist, AWS will throw
                // a NotFoundException.
                var getTopicAttributesRequest = new GetTopicAttributesRequest
                {
                    TopicArn = topicArn
                };

                await ((IAmazonSimpleNotificationService)this).GetTopicAttributesAsync(getTopicAttributesRequest, cancellationToken);

                topicExists = true;
            }
            catch (NotFoundException)
            {
                topicExists = false;
            }

            if (topicExists)
            {
                return new CreateTopicResponse
                {
                    TopicArn = topicArn,
                    HttpStatusCode = HttpStatusCode.OK
                };
            }

            throw new InvalidOperationException("Cannot create a new topic with the same name as a read-only topic");
        }
        return await _innerClient.CreateTopicAsync(request, cancellationToken);
    }

    async Task<string> IAmazonSimpleNotificationService.SubscribeQueueAsync(string topicArn, ICoreAmazonSQS sqsClient, string sqsQueueUrl)
    {
        // Convert queue URL to ARN
        var attributes = await sqsClient.GetAttributesAsync(sqsQueueUrl);
        string? queueArn = attributes["QueueArn"];

        if (_readonlyTopicArns.Contains(topicArn, StringComparer.OrdinalIgnoreCase))
        {
            // Return the ARN of the existing subscription
            var subscriptions = await _innerClient.ListSubscriptionsByTopicAsync(topicArn);

            foreach (var subscription in subscriptions.Subscriptions.Where(subscription => subscription.Endpoint == queueArn))
            {
                return subscription.SubscriptionArn;
            }

            throw new InvalidOperationException("Cannot create a new subscription to a read-only topic or queue");
        }

        return await _innerClient.SubscribeQueueAsync(topicArn, sqsClient, sqsQueueUrl);
    }

    Task<AddPermissionResponse> IAmazonSimpleNotificationService.AddPermissionAsync(AddPermissionRequest request, CancellationToken cancellationToken)
    {
        if (_readonlyTopicArns.Contains(request.TopicArn, StringComparer.OrdinalIgnoreCase))
        {
            return Task.FromResult(new AddPermissionResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            });
        }

        return _innerClient.AddPermissionAsync(request, cancellationToken);
    }

    Task<RemovePermissionResponse> IAmazonSimpleNotificationService.RemovePermissionAsync(RemovePermissionRequest request, CancellationToken cancellationToken)
    {
        if (_readonlyTopicArns.Contains(request.TopicArn, StringComparer.OrdinalIgnoreCase))
        {
            return Task.FromResult(new RemovePermissionResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            });
        }

        return _innerClient.RemovePermissionAsync(request, cancellationToken);
    }

    Task<SetSubscriptionAttributesResponse> IAmazonSimpleNotificationService.SetSubscriptionAttributesAsync(SetSubscriptionAttributesRequest request, CancellationToken cancellationToken)
    {
        if (_readonlyTopicArns.Any(x => request.SubscriptionArn.StartsWith(x + ':', StringComparison.OrdinalIgnoreCase)))
        {
            return Task.FromResult(new SetSubscriptionAttributesResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            });
        }

        return _innerClient.SetSubscriptionAttributesAsync(request, cancellationToken);
    }

    Task<SetTopicAttributesResponse> IAmazonSimpleNotificationService.SetTopicAttributesAsync(SetTopicAttributesRequest request, CancellationToken cancellationToken)
    {
        if (_readonlyTopicArns.Contains(request.TopicArn, StringComparer.OrdinalIgnoreCase))
        {
            return Task.FromResult(new SetTopicAttributesResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            });
        }

        return _innerClient.SetTopicAttributesAsync(request, cancellationToken);
    }

    Task<UnsubscribeResponse> IAmazonSimpleNotificationService.UnsubscribeAsync(UnsubscribeRequest request, CancellationToken cancellationToken)
    {
        if (_readonlyTopicArns.Any(x => request.SubscriptionArn.StartsWith(x + ':', StringComparison.OrdinalIgnoreCase)))
        {
            return Task.FromResult(new UnsubscribeResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            });
        }

        return _innerClient.UnsubscribeAsync(request, cancellationToken);
    }

    Task<UntagResourceResponse> IAmazonSimpleNotificationService.UntagResourceAsync(UntagResourceRequest request, CancellationToken cancellationToken)
    {
        return _innerClient.UntagResourceAsync(request, cancellationToken);
    }

    Task<VerifySMSSandboxPhoneNumberResponse> IAmazonSimpleNotificationService.VerifySMSSandboxPhoneNumberAsync(VerifySMSSandboxPhoneNumberRequest request, CancellationToken cancellationToken)
    {
        return _innerClient.VerifySMSSandboxPhoneNumberAsync(request, cancellationToken);
    }

    Endpoint IAmazonSimpleNotificationService.DetermineServiceOperationEndpoint(AmazonWebServiceRequest request)
    {
        return _innerClient.DetermineServiceOperationEndpoint(request);
    }

    ISimpleNotificationServicePaginatorFactory IAmazonSimpleNotificationService.Paginators => _innerClient.Paginators;

    async Task<SubscribeResponse> IAmazonSimpleNotificationService.SubscribeAsync(SubscribeRequest request, CancellationToken cancellationToken)
    {
        if (_readonlyTopicArns.Contains(request.TopicArn, StringComparer.OrdinalIgnoreCase) &&
            string.Equals(request.Protocol, "sqs", StringComparison.OrdinalIgnoreCase))
        {
            // Return the ARN of the existing subscription
            var subscriptions = await _innerClient.ListSubscriptionsByTopicAsync(request.TopicArn, cancellationToken);

            foreach (var subscription in subscriptions.Subscriptions.Where(subscription => subscription.Endpoint == request.Endpoint))
            {
                return new SubscribeResponse
                {
                    SubscriptionArn = subscription.SubscriptionArn,
                    HttpStatusCode = HttpStatusCode.OK
                };
            }

            throw new InvalidOperationException("Cannot create a new subscription to a read-only topic or queue");
        }

        return await _innerClient.SubscribeAsync(request, cancellationToken);
    }

    Task<TagResourceResponse> IAmazonSimpleNotificationService.TagResourceAsync(TagResourceRequest request, CancellationToken cancellationToken)
    {
        return _innerClient.TagResourceAsync(request, cancellationToken);
    }

    Task<DeleteTopicResponse> IAmazonSimpleNotificationService.DeleteTopicAsync(DeleteTopicRequest request, CancellationToken cancellationToken)
    {
        if (_readonlyTopicArns.Contains(request.TopicArn, StringComparer.OrdinalIgnoreCase))
        {
            return Task.FromResult(new DeleteTopicResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            });
        }

        return _innerClient.DeleteTopicAsync(request, cancellationToken);
    }

    Task<GetDataProtectionPolicyResponse> IAmazonSimpleNotificationService.GetDataProtectionPolicyAsync(GetDataProtectionPolicyRequest request, CancellationToken cancellationToken) => _innerClient.GetDataProtectionPolicyAsync(request, cancellationToken);

    private readonly IAmazonSimpleNotificationService _innerClient;
    private readonly List<string> _readonlyTopicArns;

    void IDisposable.Dispose() => _innerClient.Dispose();
    IClientConfig IAmazonService.Config => _innerClient.Config;
    Task<IDictionary<string, string>> IAmazonSimpleNotificationService.SubscribeQueueToTopicsAsync(IList<string> topicArns, ICoreAmazonSQS sqsClient, string sqsQueueUrl) => _innerClient.SubscribeQueueToTopicsAsync(topicArns, sqsClient, sqsQueueUrl);
    Task<Topic> IAmazonSimpleNotificationService.FindTopicAsync(string topicName) => _innerClient.FindTopicAsync(topicName);
    Task IAmazonSimpleNotificationService.AuthorizeS3ToPublishAsync(string topicArn, string bucket) => _innerClient.AuthorizeS3ToPublishAsync(topicArn, bucket);
    Task<AddPermissionResponse> IAmazonSimpleNotificationService.AddPermissionAsync(string topicArn, string label, List<string> awsAccountId, List<string> actionName, CancellationToken cancellationToken) => _innerClient.AddPermissionAsync(topicArn, label, awsAccountId, actionName, cancellationToken);
    Task<CheckIfPhoneNumberIsOptedOutResponse> IAmazonSimpleNotificationService.CheckIfPhoneNumberIsOptedOutAsync(CheckIfPhoneNumberIsOptedOutRequest request, CancellationToken cancellationToken) => _innerClient.CheckIfPhoneNumberIsOptedOutAsync(request, cancellationToken);
    Task<ConfirmSubscriptionResponse> IAmazonSimpleNotificationService.ConfirmSubscriptionAsync(string topicArn, string token, string authenticateOnUnsubscribe, CancellationToken cancellationToken) => _innerClient.ConfirmSubscriptionAsync(topicArn, token, authenticateOnUnsubscribe, cancellationToken);
    Task<ConfirmSubscriptionResponse> IAmazonSimpleNotificationService.ConfirmSubscriptionAsync(string topicArn, string token, CancellationToken cancellationToken) => _innerClient.ConfirmSubscriptionAsync(topicArn, token, cancellationToken);
    Task<ConfirmSubscriptionResponse> IAmazonSimpleNotificationService.ConfirmSubscriptionAsync(ConfirmSubscriptionRequest request, CancellationToken cancellationToken) => _innerClient.ConfirmSubscriptionAsync(request, cancellationToken);
    Task<CreatePlatformApplicationResponse> IAmazonSimpleNotificationService.CreatePlatformApplicationAsync(CreatePlatformApplicationRequest request, CancellationToken cancellationToken) => _innerClient.CreatePlatformApplicationAsync(request, cancellationToken);
    Task<CreatePlatformEndpointResponse> IAmazonSimpleNotificationService.CreatePlatformEndpointAsync(CreatePlatformEndpointRequest request, CancellationToken cancellationToken) => _innerClient.CreatePlatformEndpointAsync(request, cancellationToken);
    Task<CreateSMSSandboxPhoneNumberResponse> IAmazonSimpleNotificationService.CreateSMSSandboxPhoneNumberAsync(CreateSMSSandboxPhoneNumberRequest request, CancellationToken cancellationToken) => _innerClient.CreateSMSSandboxPhoneNumberAsync(request, cancellationToken);
    Task<CreateTopicResponse> IAmazonSimpleNotificationService.CreateTopicAsync(string name, CancellationToken cancellationToken) => _innerClient.CreateTopicAsync(name, cancellationToken);
    Task<DeleteEndpointResponse> IAmazonSimpleNotificationService.DeleteEndpointAsync(DeleteEndpointRequest request, CancellationToken cancellationToken) => _innerClient.DeleteEndpointAsync(request, cancellationToken);
    Task<DeletePlatformApplicationResponse> IAmazonSimpleNotificationService.DeletePlatformApplicationAsync(DeletePlatformApplicationRequest request, CancellationToken cancellationToken) => _innerClient.DeletePlatformApplicationAsync(request, cancellationToken);
    Task<DeleteSMSSandboxPhoneNumberResponse> IAmazonSimpleNotificationService.DeleteSMSSandboxPhoneNumberAsync(DeleteSMSSandboxPhoneNumberRequest request, CancellationToken cancellationToken) => _innerClient.DeleteSMSSandboxPhoneNumberAsync(request, cancellationToken);
    Task<DeleteTopicResponse> IAmazonSimpleNotificationService.DeleteTopicAsync(string topicArn, CancellationToken cancellationToken) => _innerClient.DeleteTopicAsync(topicArn, cancellationToken);
    Task<GetEndpointAttributesResponse> IAmazonSimpleNotificationService.GetEndpointAttributesAsync(GetEndpointAttributesRequest request, CancellationToken cancellationToken) => _innerClient.GetEndpointAttributesAsync(request, cancellationToken);
    Task<GetPlatformApplicationAttributesResponse> IAmazonSimpleNotificationService.GetPlatformApplicationAttributesAsync(GetPlatformApplicationAttributesRequest request, CancellationToken cancellationToken) => _innerClient.GetPlatformApplicationAttributesAsync(request, cancellationToken);
    Task<GetSMSAttributesResponse> IAmazonSimpleNotificationService.GetSMSAttributesAsync(GetSMSAttributesRequest request, CancellationToken cancellationToken) => _innerClient.GetSMSAttributesAsync(request, cancellationToken);
    Task<GetSMSSandboxAccountStatusResponse> IAmazonSimpleNotificationService.GetSMSSandboxAccountStatusAsync(GetSMSSandboxAccountStatusRequest request, CancellationToken cancellationToken) => _innerClient.GetSMSSandboxAccountStatusAsync(request, cancellationToken);
    Task<GetSubscriptionAttributesResponse> IAmazonSimpleNotificationService.GetSubscriptionAttributesAsync(string subscriptionArn, CancellationToken cancellationToken) => _innerClient.GetSubscriptionAttributesAsync(subscriptionArn, cancellationToken);
    Task<GetSubscriptionAttributesResponse> IAmazonSimpleNotificationService.GetSubscriptionAttributesAsync(GetSubscriptionAttributesRequest request, CancellationToken cancellationToken) => _innerClient.GetSubscriptionAttributesAsync(request, cancellationToken);
    Task<GetTopicAttributesResponse> IAmazonSimpleNotificationService.GetTopicAttributesAsync(string topicArn, CancellationToken cancellationToken) => _innerClient.GetTopicAttributesAsync(topicArn, cancellationToken);
    Task<GetTopicAttributesResponse> IAmazonSimpleNotificationService.GetTopicAttributesAsync(GetTopicAttributesRequest request, CancellationToken cancellationToken) => _innerClient.GetTopicAttributesAsync(request, cancellationToken);
    Task<ListEndpointsByPlatformApplicationResponse> IAmazonSimpleNotificationService.ListEndpointsByPlatformApplicationAsync(ListEndpointsByPlatformApplicationRequest request, CancellationToken cancellationToken) => _innerClient.ListEndpointsByPlatformApplicationAsync(request, cancellationToken);
    Task<ListOriginationNumbersResponse> IAmazonSimpleNotificationService.ListOriginationNumbersAsync(ListOriginationNumbersRequest request, CancellationToken cancellationToken) => _innerClient.ListOriginationNumbersAsync(request, cancellationToken);
    Task<ListPhoneNumbersOptedOutResponse> IAmazonSimpleNotificationService.ListPhoneNumbersOptedOutAsync(ListPhoneNumbersOptedOutRequest request, CancellationToken cancellationToken) => _innerClient.ListPhoneNumbersOptedOutAsync(request, cancellationToken);
    Task<ListPlatformApplicationsResponse> IAmazonSimpleNotificationService.ListPlatformApplicationsAsync(CancellationToken cancellationToken) => _innerClient.ListPlatformApplicationsAsync(cancellationToken);
    Task<ListPlatformApplicationsResponse> IAmazonSimpleNotificationService.ListPlatformApplicationsAsync(ListPlatformApplicationsRequest request, CancellationToken cancellationToken) => _innerClient.ListPlatformApplicationsAsync(request, cancellationToken);
    Task<ListSMSSandboxPhoneNumbersResponse> IAmazonSimpleNotificationService.ListSMSSandboxPhoneNumbersAsync(ListSMSSandboxPhoneNumbersRequest request, CancellationToken cancellationToken) => _innerClient.ListSMSSandboxPhoneNumbersAsync(request, cancellationToken);
    Task<ListSubscriptionsResponse> IAmazonSimpleNotificationService.ListSubscriptionsAsync(CancellationToken cancellationToken) => _innerClient.ListSubscriptionsAsync(cancellationToken);
    Task<ListSubscriptionsResponse> IAmazonSimpleNotificationService.ListSubscriptionsAsync(string nextToken, CancellationToken cancellationToken) => _innerClient.ListSubscriptionsAsync(nextToken, cancellationToken);
    Task<ListSubscriptionsResponse> IAmazonSimpleNotificationService.ListSubscriptionsAsync(ListSubscriptionsRequest request, CancellationToken cancellationToken) => _innerClient.ListSubscriptionsAsync(request, cancellationToken);
    Task<ListSubscriptionsByTopicResponse> IAmazonSimpleNotificationService.ListSubscriptionsByTopicAsync(string topicArn, string nextToken, CancellationToken cancellationToken) => _innerClient.ListSubscriptionsByTopicAsync(topicArn, nextToken, cancellationToken);
    Task<ListSubscriptionsByTopicResponse> IAmazonSimpleNotificationService.ListSubscriptionsByTopicAsync(string topicArn, CancellationToken cancellationToken) => _innerClient.ListSubscriptionsByTopicAsync(topicArn, cancellationToken);
    Task<ListSubscriptionsByTopicResponse> IAmazonSimpleNotificationService.ListSubscriptionsByTopicAsync(ListSubscriptionsByTopicRequest request, CancellationToken cancellationToken) => _innerClient.ListSubscriptionsByTopicAsync(request, cancellationToken);
    Task<ListTagsForResourceResponse> IAmazonSimpleNotificationService.ListTagsForResourceAsync(ListTagsForResourceRequest request, CancellationToken cancellationToken) => _innerClient.ListTagsForResourceAsync(request, cancellationToken);
    Task<ListTopicsResponse> IAmazonSimpleNotificationService.ListTopicsAsync(CancellationToken cancellationToken) => _innerClient.ListTopicsAsync(cancellationToken);
    Task<ListTopicsResponse> IAmazonSimpleNotificationService.ListTopicsAsync(string nextToken, CancellationToken cancellationToken) => _innerClient.ListTopicsAsync(nextToken, cancellationToken);
    Task<ListTopicsResponse> IAmazonSimpleNotificationService.ListTopicsAsync(ListTopicsRequest request, CancellationToken cancellationToken) => _innerClient.ListTopicsAsync(request, cancellationToken);
    Task<OptInPhoneNumberResponse> IAmazonSimpleNotificationService.OptInPhoneNumberAsync(OptInPhoneNumberRequest request, CancellationToken cancellationToken) => _innerClient.OptInPhoneNumberAsync(request, cancellationToken);
    Task<PublishResponse> IAmazonSimpleNotificationService.PublishAsync(string topicArn, string message, CancellationToken cancellationToken) => _innerClient.PublishAsync(topicArn, message, cancellationToken);
    Task<PublishResponse> IAmazonSimpleNotificationService.PublishAsync(string topicArn, string message, string subject, CancellationToken cancellationToken) => _innerClient.PublishAsync(topicArn, message, subject, cancellationToken);
    Task<PublishResponse> IAmazonSimpleNotificationService.PublishAsync(PublishRequest request, CancellationToken cancellationToken) => _innerClient.PublishAsync(request, cancellationToken);
    Task<PublishBatchResponse> IAmazonSimpleNotificationService.PublishBatchAsync(PublishBatchRequest request, CancellationToken cancellationToken) => _innerClient.PublishBatchAsync(request, cancellationToken);
    Task<PutDataProtectionPolicyResponse> IAmazonSimpleNotificationService.PutDataProtectionPolicyAsync(PutDataProtectionPolicyRequest request, CancellationToken cancellationToken) => _innerClient.PutDataProtectionPolicyAsync(request, cancellationToken);
    Task<RemovePermissionResponse> IAmazonSimpleNotificationService.RemovePermissionAsync(string topicArn, string label, CancellationToken cancellationToken) => _innerClient.RemovePermissionAsync(topicArn, label, cancellationToken);
    Task<SetEndpointAttributesResponse> IAmazonSimpleNotificationService.SetEndpointAttributesAsync(SetEndpointAttributesRequest request, CancellationToken cancellationToken) => _innerClient.SetEndpointAttributesAsync(request, cancellationToken);
    Task<SetPlatformApplicationAttributesResponse> IAmazonSimpleNotificationService.SetPlatformApplicationAttributesAsync(SetPlatformApplicationAttributesRequest request, CancellationToken cancellationToken) => _innerClient.SetPlatformApplicationAttributesAsync(request, cancellationToken);
    Task<SetSMSAttributesResponse> IAmazonSimpleNotificationService.SetSMSAttributesAsync(SetSMSAttributesRequest request, CancellationToken cancellationToken) => _innerClient.SetSMSAttributesAsync(request, cancellationToken);
    Task<SetSubscriptionAttributesResponse> IAmazonSimpleNotificationService.SetSubscriptionAttributesAsync(string subscriptionArn, string attributeName, string attributeValue, CancellationToken cancellationToken) => _innerClient.SetSubscriptionAttributesAsync(subscriptionArn, attributeName, attributeValue, cancellationToken);
    Task<SetTopicAttributesResponse> IAmazonSimpleNotificationService.SetTopicAttributesAsync(string topicArn, string attributeName, string attributeValue, CancellationToken cancellationToken) => _innerClient.SetTopicAttributesAsync(topicArn, attributeName, attributeValue, cancellationToken);
    Task<SubscribeResponse> IAmazonSimpleNotificationService.SubscribeAsync(string topicArn, string protocol, string endpoint, CancellationToken cancellationToken) => _innerClient.SubscribeAsync(topicArn, protocol, endpoint, cancellationToken);
    Task<UnsubscribeResponse> IAmazonSimpleNotificationService.UnsubscribeAsync(string subscriptionArn, CancellationToken cancellationToken) => _innerClient.UnsubscribeAsync(subscriptionArn, cancellationToken);
}
