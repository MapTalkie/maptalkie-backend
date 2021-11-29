using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MapTalkie.Services.Posts.Models
{
    public class PostComment
    {
        public long Id { get; set; }
        public string SenderId { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = default!;
        public DateTime UpdatedAt { get; set; } = default!;
        public bool Available { get; set; } = true;
        public long PostId { get; set; }
        public long? ReplyToId { get; set; }

        [IgnoreDataMember] public ICollection<PostComment> Comments { get; set; } = default!;
    }
}