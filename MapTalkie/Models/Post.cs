using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace MapTalkie.Models
{
    public class Post
    {
        public long Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!; // will be initialized by EF
        public Point Location { get; set; } = default!;


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = null;
        public bool Available { get; set; } = true;
        public bool IsOriginalLocation { get; set; } = true;

        public ICollection<PostComment> Comments { get; set; } = default!;
        public ICollection<PostLike> Likes { get; set; } = default!;
    }
}