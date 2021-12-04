using System.Threading.Tasks;

namespace MapTalkie.Services.FriendshipService
{
    public interface IFriendshipService
    {
        Task EnsureFriendshipRequest(string fromId, string toId);
        Task RevokeFriendship(string fromId, string toId);
        Task<FriendshipsView> FindFriendships(string userId);
    }
}