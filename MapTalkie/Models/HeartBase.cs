using System;

namespace MapTalkie.Models
{
    public class HeartBase
    {
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}