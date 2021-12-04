using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.DB
{
    public class BlacklistedUser
    {
        public string UserId { get; set; } = string.Empty;
        public string BlockedByUserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public User BlockedByUser { get; set; } = null!;
    }

    public static class BlockedUserDbSetExtensions
    {
        public static Task<bool> IsBlacklisted(this DbSet<BlacklistedUser> set, string userId1, string userId2)
            => set.Where(b => (b.UserId == userId1 && b.BlockedByUserId == userId2) ||
                              (b.UserId == userId2 && b.BlockedByUserId == userId1)).AnyAsync();
    }
}