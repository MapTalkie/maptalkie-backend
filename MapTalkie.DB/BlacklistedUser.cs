using System.Runtime.Serialization;

namespace MapTalkie.DB
{
    public class BlacklistedUser
    {
        [IgnoreDataMember] public User User { get; set; } = default!;
        public string UserId { get; set; } = string.Empty;
        [IgnoreDataMember] public User BlacklistedBy { get; set; } = default!;
        public string BlacklistedById { get; set; } = string.Empty;
    }
}