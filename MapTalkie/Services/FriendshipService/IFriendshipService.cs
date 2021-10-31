using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Models;

namespace MapTalkie.Services.FriendshipService
{
    public interface IFriendshipService
    {
        Task EnsureFriendshipRequest(int fromId, int toId);
        Task RevokeFriendship(int fromId, int toId);
        Task<bool> AreFriends(int user1Id, int user2Id);
        Task<bool> SentFriendRequest(int fromId, int toId);

        IQueryable<User> QueryFriends(int userId);
    }
}