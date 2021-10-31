using System.Collections.Generic;
using System.Threading.Tasks;
using MapTalkie.Models;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Services.FriendshipService
{
    public static class FriendshipServiceExtensions
    {
        public static Task<List<User>> GetFriends(this IFriendshipService service, int userId)
            => service.QueryFriends(userId).ToListAsync();
    }
}