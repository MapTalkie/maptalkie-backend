using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Models;
using MapTalkie.Models.Context;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Services.FriendshipService
{
    public class FriendshipService : DbService, IFriendshipService
    {
        public FriendshipService(AppDbContext context) : base(context)
        {
        }

        public async Task EnsureFriendshipRequest(int fromId, int toId)
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

        public async Task RevokeFriendship(int fromId, int toId)
        {
            var fr = await GetFriendshipRecord(fromId, toId);
            if (fr != null)
            {
                DbContext.FriendRequests.Remove(fr);
                await DbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> AreFriends(int user1Id, int user2Id)
        {
            var count = await DbContext.FriendRequests
                .Where(fr =>
                    (fr.FromId == user1Id && fr.ToId == user2Id) ||
                    (fr.FromId == user2Id && fr.ToId == user1Id))
                .CountAsync();
            return count == 2;
        }

        public Task<bool> SentFriendRequest(int fromId, int toId)
        {
            return DbContext.FriendRequests.Where(fr => fr.FromId == fromId && fr.ToId == toId).AnyAsync();
        }

        public IQueryable<User> QueryFriends(int userId)
        {
            // TODO тесты, тесты и еще раз тесты
            return
                from user in DbContext.Users
                join fr1 in DbContext.FriendRequests
                    on user.Id equals fr1.FromId
                join fr2 in DbContext.FriendRequests
                    on user.Id equals fr2.ToId
                where fr1.ToId == userId && fr2.FromId == userId
                select user;
        }

        private Task<FriendRequest?> GetFriendshipRecord(int fromId, int toId)
        {
            return DbContext.FriendRequests
                .Where(fr => fr.FromId == fromId && fr.ToId == toId)
                .FirstOrDefaultAsync()!;
        }

        private SortedUserIds SortIds(int user1, int user2)
        {
            if (user1 > user2)
            {
                (user1, user2) = (user2, user1);
            }

            return new SortedUserIds(user1, user2);
        }

        private struct SortedUserIds
        {
            public int User1;
            public int User2;

            public SortedUserIds(int user1, int user2)
            {
                User1 = user1;
                User2 = user2;
            }
        }
    }
}