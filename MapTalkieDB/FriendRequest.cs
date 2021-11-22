namespace MapTalkieDB
{
    public class FriendRequest
    {
        public string FromId { get; set; } = string.Empty;
        public string ToId { get; set; } = string.Empty;

        public User To { get; set; } = default!;
        public User From { get; set; } = default!;
    }
}