using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using NetTopologySuite.Geometries;

namespace MapTalkie.Services.Posts.Models
{
    public class Post
    {
        public long Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public Point Location { get; set; } = default!; // in Web Mercator
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool Available { get; set; } = true;
        public bool IsOriginalLocation { get; set; } = true;
        public long? SharedId { get; set; }
        public Post? Shared { get; set; }

        public int CachedLikesCount { get; set; }
        public int CachedSharesCount { get; set; }
        public int CachedCommentsCount { get; set; } = 0;
        public double CachedFreshRank { get; set; } = 0.0;
        public DateTime CacheUpdatedAt { get; set; } = DateTime.UtcNow;

        [IgnoreDataMember] public ICollection<PostComment> Comments { get; set; } = default!;
        [IgnoreDataMember] public ICollection<PostLike> Likes { get; set; } = default!;
        [IgnoreDataMember] public ICollection<Post> Shares { get; set; } = default!;
    }
}