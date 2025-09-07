using JustSaying.Messaging;
using JustSaying.Messaging.Compression;
using JustSaying.AwsTools.MessageHandling;
using JustSaying.Messaging.MessageSerialization;
using JustSaying.TestingFramework;

namespace JustSaying.UnitTests.AwsTools.MessageHandling;

public class MessageCompressionRegistryTests
{
    private readonly MessageCompressionRegistry _compressionRegistry = new([new GzipMessageBodyCompression()]);
    private readonly SystemTextJsonMessageBodySerializer<SimpleMessage> _bodySerializer = new(SystemTextJsonMessageBodySerializer.DefaultJsonSerializerOptions);

    [Test]
    public async Task CompressMessageIfNeeded_NoCompression_ReturnsOriginalMessage()
    {
        // Arrange
        var message = "Test message";
        var metadata = new PublishMetadata();
        var compressionOptions = new PublishCompressionOptions();
        var messageConverter = new OutboundMessageConverter(PublishDestinationType.Topic, _bodySerializer, _compressionRegistry, compressionOptions, "TestSubject", false);

        // Act
        var result = messageConverter.CompressMessageBody(message, metadata);

        // Assert
        await Assert.That(result.compressedMessage).IsNull();
        await Assert.That(result.contentEncoding).IsNull();
    }

    [Test]
    public async Task CompressMessageIfNeeded_CompressionThresholdNotMet_ReturnsOriginalMessage()
    {
        // Arrange
        var message = "Short message";
        var metadata = new PublishMetadata();
        var compressionOptions = new PublishCompressionOptions
        {
            CompressionEncoding = ContentEncodings.GzipBase64,
            MessageLengthThreshold = 1000
        };
        var messageConverter = new OutboundMessageConverter(PublishDestinationType.Topic, _bodySerializer, _compressionRegistry, compressionOptions, "TestSubject", false);

        // Act
        var result = messageConverter.CompressMessageBody(message, metadata);

        // Assert
        await Assert.That(result.compressedMessage).IsNull();
        await Assert.That(result.contentEncoding).IsNull();
    }

    [Test]
    public async Task CompressMessageIfNeeded_CompressionThresholdMet_ReturnsCompressedMessage()
    {
        // Arrange
        var message = new string('a', 1000);
        var metadata = new PublishMetadata();
        var compressionOptions = new PublishCompressionOptions
        {
            CompressionEncoding = ContentEncodings.GzipBase64,
            MessageLengthThreshold = 500
        };
        var messageConverter = new OutboundMessageConverter(PublishDestinationType.Topic, _bodySerializer, _compressionRegistry, compressionOptions, "TestSubject", false);

        // Act
        var result = messageConverter.CompressMessageBody(message, metadata);

        // Assert
        await Assert.That(result.compressedMessage).IsNotNull();
        await Assert.That(result.contentEncoding).IsEqualTo(ContentEncodings.GzipBase64);

        // Verify that the compressed message can be decompressed
        var gzipCompression = new GzipMessageBodyCompression();
        var decompressedMessage = gzipCompression.Decompress(result.compressedMessage);
        await Assert.That(decompressedMessage).IsEqualTo(message);
    }

    [Test]
    public async Task CompressMessageIfNeeded_WithRawMessage_CompressionThresholdMet_ReturnsCompressedMessage()
    {
        // Arrange
        var message = new string('a', 1000);
        var metadata = new PublishMetadata();
        var compressionOptions = new PublishCompressionOptions
        {
            CompressionEncoding = ContentEncodings.GzipBase64,
            MessageLengthThreshold = 500
        };
        var messageConverter = new OutboundMessageConverter(PublishDestinationType.Queue, _bodySerializer, _compressionRegistry, compressionOptions, "TestSubject", true);

        // Act
        var result = messageConverter.CompressMessageBody(message, metadata);

        // Assert
        await Assert.That(result.compressedMessage).IsNotNull();
        await Assert.That(result.contentEncoding).IsEqualTo(ContentEncodings.GzipBase64);

        // Verify that the compressed message can be decompressed
        var gzipCompression = new GzipMessageBodyCompression();
        var decompressedMessage = gzipCompression.Decompress(result.compressedMessage);
        await Assert.That(decompressedMessage).IsEqualTo(message);
    }

    [Test]
    public async Task CompressMessageIfNeeded_WithMessageAttributes_CalculatesTotalSize()
    {
        // Arrange
        var message = "Test message";
        var metadata = new PublishMetadata();
        metadata.AddMessageAttribute("Key1", new MessageAttributeValue { StringValue = "Value1", DataType = "String" });
        metadata.AddMessageAttribute("Key2", new MessageAttributeValue { BinaryValue = new byte[100], DataType = "Binary" });

        var compressionOptions = new PublishCompressionOptions
        {
            CompressionEncoding = ContentEncodings.GzipBase64,
            MessageLengthThreshold = 50
        };
        var messageConverter = new OutboundMessageConverter(PublishDestinationType.Topic, _bodySerializer, _compressionRegistry, compressionOptions, "TestSubject", false);

        // Act
        var result = messageConverter.CompressMessageBody(message, metadata);

        // Assert
        await Assert.That(result.compressedMessage).IsNotNull();
        await Assert.That(result.contentEncoding).IsEqualTo(ContentEncodings.GzipBase64);

        // Verify that the compressed message can be decompressed
        var gzipCompression = new GzipMessageBodyCompression();
        var decompressedMessage = gzipCompression.Decompress(result.compressedMessage);
        await Assert.That(decompressedMessage).IsEqualTo(message);
    }
}
