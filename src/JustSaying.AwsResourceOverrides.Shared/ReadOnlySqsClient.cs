using System.Net;
using Amazon.Runtime;
using Amazon.Runtime.SharedInterfaces;
using Amazon.SQS;
using Amazon.SQS.Model;
using AddPermissionRequest = Amazon.SQS.Model.AddPermissionRequest;
using AddPermissionResponse = Amazon.SQS.Model.AddPermissionResponse;
using Endpoint = Amazon.Runtime.Endpoints.Endpoint;
using RemovePermissionRequest = Amazon.SQS.Model.RemovePermissionRequest;
using RemovePermissionResponse = Amazon.SQS.Model.RemovePermissionResponse;

namespace JustSaying.AwsResourceOverrides;

public sealed class ReadOnlySqsClient : IAmazonSQS
{
    public ReadOnlySqsClient(IAmazonSQS innerSqsClient, IReadOnlyCollection<string> readonlyQueueUrls)
    {
        _amazonSqsImplementation = innerSqsClient;
        _readonlyQueueUrls = readonlyQueueUrls;
    }

    private readonly IAmazonSQS _amazonSqsImplementation;
    private readonly IReadOnlyCollection<string> _readonlyQueueUrls;

    async Task<CreateQueueResponse> IAmazonSQS.CreateQueueAsync(CreateQueueRequest request, CancellationToken cancellationToken)
    {
        if (_readonlyQueueUrls.Any(x => x.EndsWith('/' + request.QueueName, StringComparison.OrdinalIgnoreCase)))
        {
            var queueUrlResponse = await _amazonSqsImplementation.GetQueueUrlAsync(new GetQueueUrlRequest
            {
                QueueName = request.QueueName
            }, cancellationToken);

            return new CreateQueueResponse
            {
                QueueUrl = queueUrlResponse.QueueUrl,
                HttpStatusCode = queueUrlResponse.HttpStatusCode,
                ResponseMetadata = queueUrlResponse.ResponseMetadata
            };
        }
        return await _amazonSqsImplementation.CreateQueueAsync(request, cancellationToken);
    }

    Task<SetQueueAttributesResponse> IAmazonSQS.SetQueueAttributesAsync(SetQueueAttributesRequest request, CancellationToken cancellationToken)
    {
        if (_readonlyQueueUrls.Contains(request.QueueUrl, StringComparer.OrdinalIgnoreCase))
        {
            return Task.FromResult(new SetQueueAttributesResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            });
        }

        return _amazonSqsImplementation.SetQueueAttributesAsync(request, cancellationToken);
    }

    Task<TagQueueResponse> IAmazonSQS.TagQueueAsync(TagQueueRequest request, CancellationToken cancellationToken)
    {
        if (_readonlyQueueUrls.Contains(request.QueueUrl, StringComparer.OrdinalIgnoreCase))
        {
            return Task.FromResult(new TagQueueResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            });
        }
        return _amazonSqsImplementation.TagQueueAsync(request, cancellationToken);
    }

    Task<RemovePermissionResponse> IAmazonSQS.RemovePermissionAsync(RemovePermissionRequest request, CancellationToken cancellationToken)
    {
        if (_readonlyQueueUrls.Contains(request.QueueUrl, StringComparer.OrdinalIgnoreCase))
        {
            return Task.FromResult(new RemovePermissionResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            });
        }
        return _amazonSqsImplementation.RemovePermissionAsync(request, cancellationToken);
    }

    Task<AddPermissionResponse> IAmazonSQS.AddPermissionAsync(AddPermissionRequest request, CancellationToken cancellationToken)
    {
        if (_readonlyQueueUrls.Contains(request.QueueUrl, StringComparer.OrdinalIgnoreCase))
        {
            return Task.FromResult(new AddPermissionResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            });
        }
        return _amazonSqsImplementation.AddPermissionAsync(request, cancellationToken);
    }

    Task<DeleteQueueResponse> IAmazonSQS.DeleteQueueAsync(DeleteQueueRequest request, CancellationToken cancellationToken)
    {
        if (_readonlyQueueUrls.Contains(request.QueueUrl, StringComparer.OrdinalIgnoreCase))
        {
            return Task.FromResult(new DeleteQueueResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            });
        }

        return _amazonSqsImplementation.DeleteQueueAsync(request, cancellationToken);
    }

    void IDisposable.Dispose() => _amazonSqsImplementation.Dispose();
    Task<Dictionary<string, string>> ICoreAmazonSQS.GetAttributesAsync(string queueUrl) => _amazonSqsImplementation.GetAttributesAsync(queueUrl);
    Task ICoreAmazonSQS.SetAttributesAsync(string queueUrl, Dictionary<string, string> attributes) => _amazonSqsImplementation.SetAttributesAsync(queueUrl, attributes);
    IClientConfig IAmazonService.Config => _amazonSqsImplementation.Config;
    Task<string> IAmazonSQS.AuthorizeS3ToSendMessageAsync(string queueUrl, string bucket) => _amazonSqsImplementation.AuthorizeS3ToSendMessageAsync(queueUrl, bucket);
    Task<AddPermissionResponse> IAmazonSQS.AddPermissionAsync(string queueUrl, string label, List<string> awsAccountIds, List<string> actions, CancellationToken cancellationToken) => _amazonSqsImplementation.AddPermissionAsync(queueUrl, label, awsAccountIds, actions, cancellationToken);
    Task<ChangeMessageVisibilityResponse> IAmazonSQS.ChangeMessageVisibilityAsync(string queueUrl, string receiptHandle, int visibilityTimeout, CancellationToken cancellationToken) => _amazonSqsImplementation.ChangeMessageVisibilityAsync(queueUrl, receiptHandle, visibilityTimeout, cancellationToken);
    Task<ChangeMessageVisibilityResponse> IAmazonSQS.ChangeMessageVisibilityAsync(ChangeMessageVisibilityRequest request, CancellationToken cancellationToken) => _amazonSqsImplementation.ChangeMessageVisibilityAsync(request, cancellationToken);
    Task<ChangeMessageVisibilityBatchResponse> IAmazonSQS.ChangeMessageVisibilityBatchAsync(string queueUrl, List<ChangeMessageVisibilityBatchRequestEntry> entries, CancellationToken cancellationToken) => _amazonSqsImplementation.ChangeMessageVisibilityBatchAsync(queueUrl, entries, cancellationToken);
    Task<ChangeMessageVisibilityBatchResponse> IAmazonSQS.ChangeMessageVisibilityBatchAsync(ChangeMessageVisibilityBatchRequest request, CancellationToken cancellationToken) => _amazonSqsImplementation.ChangeMessageVisibilityBatchAsync(request, cancellationToken);
    Task<CreateQueueResponse> IAmazonSQS.CreateQueueAsync(string queueName, CancellationToken cancellationToken) => _amazonSqsImplementation.CreateQueueAsync(queueName, cancellationToken);
    Task<DeleteMessageResponse> IAmazonSQS.DeleteMessageAsync(string queueUrl, string receiptHandle, CancellationToken cancellationToken) => _amazonSqsImplementation.DeleteMessageAsync(queueUrl, receiptHandle, cancellationToken);
    Task<DeleteMessageResponse> IAmazonSQS.DeleteMessageAsync(DeleteMessageRequest request, CancellationToken cancellationToken) => _amazonSqsImplementation.DeleteMessageAsync(request, cancellationToken);
    Task<DeleteMessageBatchResponse> IAmazonSQS.DeleteMessageBatchAsync(string queueUrl, List<DeleteMessageBatchRequestEntry> entries, CancellationToken cancellationToken) => _amazonSqsImplementation.DeleteMessageBatchAsync(queueUrl, entries, cancellationToken);
    Task<DeleteMessageBatchResponse> IAmazonSQS.DeleteMessageBatchAsync(DeleteMessageBatchRequest request, CancellationToken cancellationToken) => _amazonSqsImplementation.DeleteMessageBatchAsync(request, cancellationToken);
    Task<DeleteQueueResponse> IAmazonSQS.DeleteQueueAsync(string queueUrl, CancellationToken cancellationToken) => _amazonSqsImplementation.DeleteQueueAsync(queueUrl, cancellationToken);
    Task<GetQueueAttributesResponse> IAmazonSQS.GetQueueAttributesAsync(string queueUrl, List<string> attributeNames, CancellationToken cancellationToken) => _amazonSqsImplementation.GetQueueAttributesAsync(queueUrl, attributeNames, cancellationToken);
    Task<GetQueueAttributesResponse> IAmazonSQS.GetQueueAttributesAsync(GetQueueAttributesRequest request, CancellationToken cancellationToken) => _amazonSqsImplementation.GetQueueAttributesAsync(request, cancellationToken);
    Task<GetQueueUrlResponse> IAmazonSQS.GetQueueUrlAsync(string queueName, CancellationToken cancellationToken) => _amazonSqsImplementation.GetQueueUrlAsync(queueName, cancellationToken);
    Task<GetQueueUrlResponse> IAmazonSQS.GetQueueUrlAsync(GetQueueUrlRequest request, CancellationToken cancellationToken) => _amazonSqsImplementation.GetQueueUrlAsync(request, cancellationToken);
    Task<ListDeadLetterSourceQueuesResponse> IAmazonSQS.ListDeadLetterSourceQueuesAsync(ListDeadLetterSourceQueuesRequest request, CancellationToken cancellationToken) => _amazonSqsImplementation.ListDeadLetterSourceQueuesAsync(request, cancellationToken);
    Task<ListQueuesResponse> IAmazonSQS.ListQueuesAsync(string queueNamePrefix, CancellationToken cancellationToken) => _amazonSqsImplementation.ListQueuesAsync(queueNamePrefix, cancellationToken);
    Task<ListQueuesResponse> IAmazonSQS.ListQueuesAsync(ListQueuesRequest request, CancellationToken cancellationToken) => _amazonSqsImplementation.ListQueuesAsync(request, cancellationToken);
    Task<ListQueueTagsResponse> IAmazonSQS.ListQueueTagsAsync(ListQueueTagsRequest request, CancellationToken cancellationToken) => _amazonSqsImplementation.ListQueueTagsAsync(request, cancellationToken);
    Task<PurgeQueueResponse> IAmazonSQS.PurgeQueueAsync(string queueUrl, CancellationToken cancellationToken) => _amazonSqsImplementation.PurgeQueueAsync(queueUrl, cancellationToken);
    Task<PurgeQueueResponse> IAmazonSQS.PurgeQueueAsync(PurgeQueueRequest request, CancellationToken cancellationToken) => _amazonSqsImplementation.PurgeQueueAsync(request, cancellationToken);
    Task<ReceiveMessageResponse> IAmazonSQS.ReceiveMessageAsync(string queueUrl, CancellationToken cancellationToken) => _amazonSqsImplementation.ReceiveMessageAsync(queueUrl, cancellationToken);
    Task<ReceiveMessageResponse> IAmazonSQS.ReceiveMessageAsync(ReceiveMessageRequest request, CancellationToken cancellationToken) => _amazonSqsImplementation.ReceiveMessageAsync(request, cancellationToken);
    Task<RemovePermissionResponse> IAmazonSQS.RemovePermissionAsync(string queueUrl, string label, CancellationToken cancellationToken) => _amazonSqsImplementation.RemovePermissionAsync(queueUrl, label, cancellationToken);
    Task<SendMessageResponse> IAmazonSQS.SendMessageAsync(string queueUrl, string messageBody, CancellationToken cancellationToken) => _amazonSqsImplementation.SendMessageAsync(queueUrl, messageBody, cancellationToken);
    Task<SendMessageResponse> IAmazonSQS.SendMessageAsync(SendMessageRequest request, CancellationToken cancellationToken) => _amazonSqsImplementation.SendMessageAsync(request, cancellationToken);
    Task<SendMessageBatchResponse> IAmazonSQS.SendMessageBatchAsync(string queueUrl, List<SendMessageBatchRequestEntry> entries, CancellationToken cancellationToken) => _amazonSqsImplementation.SendMessageBatchAsync(queueUrl, entries, cancellationToken);
    Task<SendMessageBatchResponse> IAmazonSQS.SendMessageBatchAsync(SendMessageBatchRequest request, CancellationToken cancellationToken) => _amazonSqsImplementation.SendMessageBatchAsync(request, cancellationToken);
    Task<SetQueueAttributesResponse> IAmazonSQS.SetQueueAttributesAsync(string queueUrl, Dictionary<string, string> attributes, CancellationToken cancellationToken) => _amazonSqsImplementation.SetQueueAttributesAsync(queueUrl, attributes, cancellationToken);
    Task<UntagQueueResponse> IAmazonSQS.UntagQueueAsync(UntagQueueRequest request, CancellationToken cancellationToken) => _amazonSqsImplementation.UntagQueueAsync(request, cancellationToken);
    Task<CancelMessageMoveTaskResponse> IAmazonSQS.CancelMessageMoveTaskAsync(CancelMessageMoveTaskRequest request, CancellationToken cancellationToken) => _amazonSqsImplementation.CancelMessageMoveTaskAsync(request, cancellationToken);
    Task<ListMessageMoveTasksResponse> IAmazonSQS.ListMessageMoveTasksAsync(ListMessageMoveTasksRequest request, CancellationToken cancellationToken) => _amazonSqsImplementation.ListMessageMoveTasksAsync(request, cancellationToken);
    Task<StartMessageMoveTaskResponse> IAmazonSQS.StartMessageMoveTaskAsync(StartMessageMoveTaskRequest request, CancellationToken cancellationToken) => _amazonSqsImplementation.StartMessageMoveTaskAsync(request, cancellationToken);
    Endpoint IAmazonSQS.DetermineServiceOperationEndpoint(AmazonWebServiceRequest request) => _amazonSqsImplementation.DetermineServiceOperationEndpoint(request);
    ISQSPaginatorFactory IAmazonSQS.Paginators => _amazonSqsImplementation.Paginators;
}
