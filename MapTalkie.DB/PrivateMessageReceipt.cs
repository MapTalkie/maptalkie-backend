using System.Runtime.Serialization;

namespace MapTalkie.DB
{
    public class PrivateMessageReceipt
    {
        public string UserIdA { get; set; }
        public string UserIdB { get; set; }
        [IgnoreDataMember] public User UserA { get; set; }
        [IgnoreDataMember] public User UserB { get; set; }
        public bool OutFlag { get; set; }

        public long MessageId { get; set; }
        public PrivateMessage Message { get; set; } = default!;
    }
}