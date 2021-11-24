using System;

namespace MapTalkie.DB
{
    public class LikeBase
    {
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}