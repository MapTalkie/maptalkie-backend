using MapTalkieCommon.Messages;

namespace MapTalkie.MessagesImpl
{
    public class PrivateMessageUpdateEvent : PrivateMessageEventBase, IPrivateMessageUpdate
    {
        public string NewText { get; set; } = string.Empty;
    }
}