using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace MapTalkieDB
{
    public class Post
    {
        public string Id { get; set; } = Nanoid.Nanoid.Generate();
        public string Text { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!; // will be initialized by EF
        public Point Location { get; set; } = default!; // in Web Mercator
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = null;
        public bool Available { get; set; } = true;
        public bool IsOriginalLocation { get; set; } = true;
        public long? SharedId { get; set; }

        public int CachedLikesCount { get; set; }
        public int CachedSharesCount { get; set; }
        public int CachedCommentsCount { get; set; } = 0;
        public double CachedFreshRank { get; set; } = 0.0;
        public DateTime CacheUpdatedAt { get; set; } = DateTime.Now;

        public ICollection<PostComment> Comments { get; set; } = default!;
        public ICollection<PostLike> Likes { get; set; } = default!;
        public ICollection<Post> Shares { get; set; } = default!;
        public Post? Shared { get; set; } = default!;
    }
}