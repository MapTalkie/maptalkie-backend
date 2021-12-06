namespace MapTalkie.Services.FriendshipService
{
    public enum FriendshipState : byte
    {
        None = 0,
        RequestPending = 1,
        IncomingRequest = 2,
        Mutual = RequestPending | IncomingRequest
    }
}