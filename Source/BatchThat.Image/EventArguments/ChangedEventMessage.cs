using BatchThat.Image.Enums;

namespace BatchThat.Image.EventArguments
{
    public class ChangedEventMessage
    {
        public string Message { get; set; }
        public EnumMessageType MessageType { get; set; }

        public ChangedEventMessage(string message, EnumMessageType messageType)
        {
            Message = message;
            MessageType = messageType;
        }
    }
}
