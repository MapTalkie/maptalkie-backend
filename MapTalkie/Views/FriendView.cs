namespace MapTalkie.Views;

public enum FriendState
{
    Mutual,
    RequestPending,
    WaitingForAcceptance
}

public record FriendView
{
    public string UserName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public FriendState State { get; set; }
}