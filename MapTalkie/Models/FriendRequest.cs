namespace MapTalkie.Models
{
    public class FriendRequest
    {
        public int FromId { get; set; }
        public int ToId { get; set; }

        public User To { get; set; } = default!;
        public User From { get; set; } = default!;
    }
}