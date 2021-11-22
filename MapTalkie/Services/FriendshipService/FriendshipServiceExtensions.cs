using System.Collections.Generic;
using System.Threading.Tasks;
using MapTalkieDB;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Services.FriendshipService
{
    public static class FriendshipServiceExtensions
    {
        public static Task<List<User>> GetFriends(this IFriendshipService service, string userId)
        {
            return service.QueryFriends(userId).ToListAsync();
        }
    }
}