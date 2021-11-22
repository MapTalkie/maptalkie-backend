using System;

namespace MapTalkieDB
{
    public class Attachment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = default!;
    }
}