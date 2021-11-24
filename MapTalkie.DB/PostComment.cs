using System;
using System.Collections.Generic;

namespace MapTalkie.DB
{
    public class PostComment
    {
        public long Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public User Sender { get; set; } = default!;

        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = default!;
        public DateTime UpdatedAt { get; set; } = default!;
        public bool Available { get; set; } = true;

        public long PostId { get; set; }
        public Post Post { get; set; } = default!;

        public long? ReplyToId { get; set; }
        public PostComment? ReplyTo { get; set; }

        public ICollection<PostComment> Comments { get; set; } = default!;
    }
}