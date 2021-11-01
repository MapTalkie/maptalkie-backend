using System;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Models;
using MapTalkie.Models.Context;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace MapTalkie.Services.PostService
{
    public class PostService : DbService, IPostService
    {
        public PostService(AppDbContext context) : base(context)
        {
        }

        public async Task<MapPost> CreateTextPost(
            string text,
            string userId,
            Point location,
            bool isOriginalLocation)
        {
            var message = new MapPost
            {
                Text = text,
                Location = location,
                UserId = userId,
                IsOriginalLocation = isOriginalLocation
            };
            DbContext.Posts.Add(message);
            return message;
        }

        public Task<MapPost?> GetPostOrNull(long id, bool includeUnavailable)
        {
            var query = DbContext.Posts.Where(m => m.Id == id);
            if (!includeUnavailable)
                query = query.Where(p => p.Available);
            return query.FirstOrDefaultAsync()!;
        }

        public IQueryable<MapPost> QueryPostsInArea(
            Geometry geometry,
            DateTime? before = null,
            User? availableFor = null)
        {
            var query =
                DbContext.Posts.Where(m => m.Available && geometry.Contains(m.Location));
            if (before != null)
                query = query.Where(m => m.CreatedAt <= before);

            if (availableFor != null)
            {
                query = from p in query
                    join b in DbContext.BlacklistedUsers
                        on p.UserId equals b.UserId into bs
                    from b in bs.DefaultIfEmpty()
                    where b.BlacklistedById != availableFor.Id
                    select p;
            }

            return query;
        }

        public Task<bool> IsAvailable(long id)
        {
            return DbContext.Posts.Where(p => p.Id == id && p.Available).AnyAsync();
        }
    }
}