using System;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Models;
using MapTalkie.Models.Context;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Services.PostService
{
    internal class PostService : DbService, IPostService
    {
        public PostService(AppDbContext context) : base(context)
        {
        }

        public async Task<MapPost> CreateTextPost(string text, User user, Location location, bool isOriginalLocation)
        {
            var message = new MapPost
            {
                Text = text,
                Loc = location,
                UserId = user.Id,
                IsOriginalLocation = isOriginalLocation
            };
            DbContext.Posts.Add(message);
            return message;
        }

        public Task<MapPost?> GetPostOrNull(long id)
        {
            return DbContext.Posts.Where(m => m.Id == id).FirstOrDefaultAsync()!;
        }

        public IQueryable<MapPost> QueryPostsInArea(
            LatLngBounds bounds,
            DateTime? before = null,
            User? availableFor = null)
        {
            var query = DbContext.Posts
                .Where(m => m.Available)
                .Where(m => (
                    m.Loc.Latitude <= bounds.NorthEast.Latitude &&
                    m.Loc.Latitude >= bounds.SouthWest.Latitude &&
                    m.Loc.Longitude <= bounds.NorthEast.Longitude &&
                    m.Loc.Longitude >= bounds.SouthWest.Longitude));
            if (before != null)
                query = query.Where(m => m.CreatedAt <= before);

            if (availableFor != null)
            {
                query = from p in query
                    join b in DbContext.BlacklistedUsers
                        on p.UserId equals b.UserId into bs
                    from b in bs.DefaultIfEmpty()
                    where b?.BlacklistedById != availableFor.Id
                    select p;
            }

            return query;
        }
    }
}