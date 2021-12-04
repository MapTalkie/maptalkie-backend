using System.Collections.Generic;

namespace MapTalkie.Services.FriendshipService
{
    public record FriendshipsView(List<FriendView> Friends, List<FriendView> RequestsPending,
        List<FriendView> IncomingRequests);

    public record FriendView(string UserId, string UserName);
}