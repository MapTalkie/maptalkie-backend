using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MapTalkie.DB
{
    public class PostComment
    {
        public long Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        [IgnoreDataMember] public User Sender { get; set; } = default!;

        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = default!;
        public DateTime UpdatedAt { get; set; } = default!;
        public bool Available { get; set; } = true;

        public long PostId { get; set; }
        [IgnoreDataMember] public Post Post { get; set; } = default!;

        public long? ReplyToId { get; set; }
        [IgnoreDataMember] public PostComment? ReplyTo { get; set; }

        [IgnoreDataMember] public ICollection<PostComment> Comments { get; set; } = default!;
    }
}