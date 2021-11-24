namespace MapTalkie.DB
{
    public class BlacklistedUser
    {
        public User User { get; set; } = default!;
        public string UserId { get; set; } = string.Empty;
        public User BlacklistedBy { get; set; } = default!;
        public string BlacklistedById { get; set; } = string.Empty;
    }
}