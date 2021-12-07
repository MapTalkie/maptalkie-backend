using System;
using System.Runtime.Serialization;

namespace MapTalkie.DB
{
    public class PrivateConversationParticipant
    {
        public string SenderId { get; set; } = default!;
        public string RecipientId { get; set; } = default!;
        [IgnoreDataMember] public User Sender { get; set; } = default!;
        [IgnoreDataMember] public User Recipient { get; set; } = default!;
        public DateTime LastMessageReadAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}