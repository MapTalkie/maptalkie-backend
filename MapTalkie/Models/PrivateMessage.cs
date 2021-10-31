namespace MapTalkie.Models
{
    public class PrivateMessage : Message
    {
        public int ConversationId { get; set; }
        public PrivateConversation Conversation { get; set; } = default!;
        public bool Read { get; set; } = false;
    }
}