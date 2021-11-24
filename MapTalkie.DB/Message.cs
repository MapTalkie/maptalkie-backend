using System;
using System.Runtime.Serialization;

namespace MapTalkie.DB
{
    public class Message
    {
        public long Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        [IgnoreDataMember] public User Sender { get; set; } = default!;
        public string RecipientId { get; set; } = string.Empty;
        [IgnoreDataMember] public User Recipient { get; set; } = default!;
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = null;
        public bool Available { get; set; } = true;
    }
}