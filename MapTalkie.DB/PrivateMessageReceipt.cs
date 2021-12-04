using System.Runtime.Serialization;

namespace MapTalkie.DB
{
    public class PrivateMessageReceipt
    {
        public string UserIdA { get; set; } = null!;
        public string UserIdB { get; set; } = null!;
        [IgnoreDataMember] public User UserA { get; set; } = null!;
        [IgnoreDataMember] public User UserB { get; set; } = null!;
        public bool OutFlag { get; set; }

        public long MessageId { get; set; }
        public PrivateMessage Message { get; set; } = default!;
    }
}