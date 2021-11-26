using System.Runtime.Serialization;

namespace MapTalkie.DB
{
    public class FriendRequest
    {
        public string FromId { get; set; } = string.Empty;
        public string ToId { get; set; } = string.Empty;

        [IgnoreDataMember] public User To { get; set; } = default!;
        [IgnoreDataMember] public User From { get; set; } = default!;
    }
}