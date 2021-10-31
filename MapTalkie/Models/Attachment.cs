using System;

namespace MapTalkie.Models
{
    public class Attachment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int UserId { get; set; }
        public User User { get; set; } = default!;
    }
}