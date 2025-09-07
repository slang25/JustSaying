using JustSaying.Messaging.MessageSerialization;
using JustSaying.TestingFramework;

namespace JustSaying.UnitTests.Messaging.Serialization.Newtonsoft;

public class WhenSerializingAndDeserializing : XBehaviourTest<NewtonsoftMessageBodySerializer<MessageWithEnum>>
{
    private MessageWithEnum _messageOut;
    private MessageWithEnum _messageIn;
    private string _jsonMessage;
    protected override void Given()
    {
        _messageOut = new MessageWithEnum { EnumVal = Value.Two };
    }

    protected override void WhenAction()
    {
        _jsonMessage = SystemUnderTest.Serialize(_messageOut);
        _messageIn = SystemUnderTest.Deserialize(_jsonMessage) as MessageWithEnum;
    }

    [Test]
    public async Task MessageHasBeenCreated()
    {
        _messageOut.ShouldNotBeNull();
    }

    [Test]
    public async Task MessagesContainSameDetails()
    {
        _messageOut.EnumVal.ShouldBe(_messageIn.EnumVal);
        _messageOut.RaisingComponent.ShouldBe(_messageIn.RaisingComponent);
        _messageOut.TimeStamp.ShouldBe(_messageIn.TimeStamp);
    }

    [Test]
    public async Task EnumsAreRepresentedAsStrings()
    {
        _jsonMessage.ShouldContain("EnumVal");
        _jsonMessage.ShouldContain("Two");
    }
}
