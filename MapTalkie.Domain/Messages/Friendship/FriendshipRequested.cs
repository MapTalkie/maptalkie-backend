namespace MapTalkie.Domain.Messages.Friendship
{
    public class FriendshipRequested : FriendshipMessage
    {
        public string RequestedById { get; set; }
    }
}