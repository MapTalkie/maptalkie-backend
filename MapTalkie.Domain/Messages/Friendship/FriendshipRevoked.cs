namespace MapTalkie.Domain.Messages.Friendship;

public class FriendshipRevoked : FriendshipMessage
{
    public string RevokedById { get; set; }
}