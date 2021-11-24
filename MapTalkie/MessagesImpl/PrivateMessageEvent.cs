using MapTalkie.Common.Messages.PrivateMessages;

namespace MapTalkie.MessagesImpl
{
    public class PrivateMessageEvent : IPrivateMessage
    {
        public int ConversationId { get; set; }
        public string SenderId { get; set; }
        public string RecipientId { get; set; }
        public long MessageId { get; set; }
        public string Text { get; }
    }
}