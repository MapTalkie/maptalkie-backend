using System;

namespace MapTalkie.Models
{
    public class PostComment
    {
        public long Id { get; set; }
        public int SenderId { get; set; }
        public User Sender { get; set; } = default!;

        public string Text { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = default!;
        public DateTime UpdatedAt { get; set; } = default!;
        public bool Available { get; set; } = true;

        public long PostId { get; set; }
        public MapPost Post { get; set; } = default!;

        public long? ReplyToId { get; set; }
        public PostComment? ReplyTo { get; set; }
    }
}