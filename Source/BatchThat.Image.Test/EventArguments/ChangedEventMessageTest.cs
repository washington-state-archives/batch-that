using BatchThat.Image.Enums;
using BatchThat.Image.EventArguments;
using Xunit;

namespace BatchThat.Image.Test.EventArguments
{
    public class ChangedEventMessageTest
    {
        [Fact]
        public void Constructor()
        {
            ChangedEventMessage changedEventMessage = new ChangedEventMessage("some", EnumMessageType.Informational);

            Assert.Equal("some", changedEventMessage.Message);
            Assert.True(changedEventMessage.MessageType == EnumMessageType.Informational);
        }
    }
}