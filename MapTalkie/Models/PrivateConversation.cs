using System;

namespace MapTalkie.Models
{
    public class PrivateConversation
    {
        public int UserLowerId { get; set; }
        public int UserHigherId { get; set; }
        public User UserLower { get; set; } = default!;
        public User UserHigher { get; set; } = default!;

        public DateTime HiddenBoundForLowerUser { get; set; } = DateTime.MinValue;
        public DateTime HiddenBoundForHigherUser { get; set; } = DateTime.MinValue;
    }
}