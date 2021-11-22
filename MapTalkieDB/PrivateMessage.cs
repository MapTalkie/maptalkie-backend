using System.Runtime.Serialization;

namespace MapTalkieDB
{
    public class PrivateMessage : Message
    {
        public int ConversationId { get; set; }
        [IgnoreDataMember] public PrivateConversation Conversation { get; set; } = default!;
        public bool Read { get; set; } = false;
    }
}