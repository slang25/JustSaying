using JustSaying.Naming;
using JustSaying.TestingFramework;

namespace JustSaying.UnitTests.Naming;

public class DefaultNamingConventionsTests
{
    private readonly DefaultNamingConventions Sut = new();

    [Test]
    public async Task WhenGeneratingTopicName_ForNonGenericType_ThenTheCorrectNameShouldBeReturned()
    {
        // Arrange + Act
        var result = Sut.TopicName<SimpleMessage>();

        // Assert
        result.ShouldBe("simplemessage");
    }

    [Test]
    public async Task WhenGeneratingTopicName_ForGenericType_ThenTheCorrectNameShouldBeReturned()
    {
        // Arrange + Act
        var result = Sut.TopicName<List<List<string>>>();

        // Assert
        result.ShouldBe("listliststring");
    }

    [Test]
    public async Task WhenGeneratingTopicName_ForTypeWithLongName_ThenTheLengthShouldBe256()
    {
        // Arrange + Act
        var result = Sut
            .TopicName<Tuple<
                TypeWithAReallyReallyReallyLongClassNameThatShouldExceedTheMaximumLengthOfAnAwsResourceName
                , TypeWithAReallyReallyReallyLongClassNameThatShouldExceedTheMaximumLengthOfAnAwsResourceName,
                TypeWithAReallyReallyReallyLongClassNameThatShouldExceedTheMaximumLengthOfAnAwsResourceName>>();

        // Arrange
        result.Length.ShouldBe(256);
    }

    [Test]
    public async Task WhenGeneratingQueueName_ForNonGenericType_ThenTheCorrectNameShouldBeReturned()
    {
        // Arrange + Act
        var result = Sut.QueueName<SimpleMessage>();

        // Assert
        result.ShouldBe("simplemessage");
    }

    [Test]
    public async Task WhenGeneratingQueueName_ForGenericType_ThenTheCorrectNameShouldBeReturned()
    {
        // Arrange + Act
        var result = Sut.QueueName<List<string>>();

        // Assert
        result.ShouldBe("liststring");
    }

    [Test]
    public async Task WhenGeneratingQueueName_ForTypeWithLongName_ThenTheLengthShouldBe80()
    {
        // Arrange + Act
        var result =
            Sut.QueueName<
                TypeWithAReallyReallyReallyLongClassNameThatShouldExceedTheMaximumLengthOfAnAwsResourceName>();

        // Assert
        result.Length.ShouldBe(80);
    }

    public class TypeWithAReallyReallyReallyLongClassNameThatShouldExceedTheMaximumLengthOfAnAwsResourceName
    {
    }
}