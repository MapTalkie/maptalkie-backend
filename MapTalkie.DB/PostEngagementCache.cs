using System;
using System.ComponentModel.DataAnnotations;

namespace MapTalkie.DB
{
    public class PostEngagementCache
    {
        [Key] public long PostId { get; set; }
        public Post Post { get; set; } = null!;

        public int LikesCount { get; set; }
        public int SharesCount { get; set; }
        public int CommentsCount { get; set; }
        public double Rank { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}