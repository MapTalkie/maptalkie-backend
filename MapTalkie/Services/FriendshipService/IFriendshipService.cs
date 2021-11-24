using System.Linq;
using System.Threading.Tasks;
using MapTalkie.DB;

namespace MapTalkie.Services.FriendshipService
{
    public interface IFriendshipService
    {
        Task EnsureFriendshipRequest(string fromId, string toId);
        Task RevokeFriendship(string fromId, string toId);
        Task<bool> AreFriends(string user1Id, string user2Id);
        Task<bool> IsFriendRequestSent(string fromId, string toId);

        IQueryable<User> QueryFriends(string userId);
    }
}