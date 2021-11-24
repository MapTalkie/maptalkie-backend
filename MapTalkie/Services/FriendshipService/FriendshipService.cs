using System.Linq;
using System.Threading.Tasks;
using MapTalkie.DB;
using MapTalkie.DB.Context;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Services.FriendshipService
{
    public class FriendshipService : DbService, IFriendshipService
    {
        public FriendshipService(AppDbContext context) : base(context)
        {
        }

        public async Task EnsureFriendshipRequest(string fromId, string toId)
        {
            var fr = await GetFriendshipRecord(fromId, toId);
            if (fr == null)
            {
                fr = new FriendRequest
                {
                    FromId = fromId,
                    ToId = toId
                };
                DbContext.FriendRequests.Add(fr);
                await DbContext.SaveChangesAsync();
            }
        }

        public async Task RevokeFriendship(string fromId, string toId)
        {
            var fr = await GetFriendshipRecord(fromId, toId);
            if (fr != null)
            {
                DbContext.FriendRequests.Remove(fr);
                await DbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> AreFriends(string user1Id, string user2Id)
        {
            var count = await DbContext.FriendRequests
                .Where(fr =>
                    fr.FromId == user1Id && fr.ToId == user2Id ||
                    fr.FromId == user2Id && fr.ToId == user1Id)
                .CountAsync();
            return count == 2;
        }

        public Task<bool> IsFriendRequestSent(string fromId, string toId)
        {
            return DbContext.FriendRequests.Where(fr => fr.FromId == fromId && fr.ToId == toId).AnyAsync();
        }

        public IQueryable<User> QueryFriends(string userId)
        {
            return
                from user in DbContext.Users
                join outRequest in DbContext.FriendRequests
                    on user.Id equals outRequest.FromId
                join inRequest in DbContext.FriendRequests
                    on user.Id equals inRequest.ToId
                where inRequest.FromId == userId && outRequest.ToId == userId
                select user;
        }

        private Task<FriendRequest?> GetFriendshipRecord(string fromId, string toId)
        {
            return DbContext.FriendRequests
                .Where(fr => fr.FromId == fromId && fr.ToId == toId)
                .FirstOrDefaultAsync()!;
        }
    }
}