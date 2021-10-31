namespace MapTalkie.Models
{
    public class BlacklistedUser
    {
        public User User { get; set; }
        public int UserId { get; set; }
        public User BlacklistedBy { get; set; }
        public int BlacklistedById { get; set; }
    }
}