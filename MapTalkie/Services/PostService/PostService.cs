using System;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Models;
using MapTalkie.Models.Context;
using MapTalkie.Services.EventBus;
using MapTalkie.Services.PostService.Events;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace MapTalkie.Services.PostService
{
    public class PostService : DbService, IPostService
    {
        private readonly IEventBus _eventBus;

        public PostService(AppDbContext context, IEventBus eventBus) : base(context)
        {
            _eventBus = eventBus;
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
            await DbContext.SaveChangesAsync();
            return message;
        }

        public Task<MapPost?> GetPostOrNull(long id, bool includeUnavailable)
        {
            var query = DbContext.Posts.Where(m => m.Id == id);
            if (!includeUnavailable)
                query = query.Where(p => p.Available);
            return query.FirstOrDefaultAsync()!;
        }

        public IQueryable<MapPost> QueryPosts(
            Geometry? geometry = null,
            bool? available = true,
            User? availableFor = null)
        {
            IQueryable<MapPost> query = DbContext.Posts;
            if (available != null)
                query = query.Where(p => p.Available == available);
            if (geometry != null)
                query = DbContext.Posts.Where(m => geometry.Contains(m.Location));

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

        public IQueryable<MapPost> QueryPopularPosts(
            int limit = PostServiceDefaults.PopularPostsLimit,
            Geometry? geometry = null,
            bool? available = true,
            User? availableFor = null)
        {
            var query = QueryPosts(geometry, available, availableFor);

            query = from p in query
                let comments = (from c in DbContext.PostComments where c.PostId == p.Id select c).Count()
                let likes = (from l in DbContext.PostLikes where l.PostId == p.Id select l).Count()
                let timeDecay = Math.Exp(-(DateTime.Now - p.CreatedAt).TotalHours)
                let rank = comments + likes * 2
                orderby rank descending
                select p;

            return query.Take(limit);
        }

        public Task<bool> IsAvailable(long id)
        {
            return DbContext.Posts.Where(p => p.Id == id && p.Available).AnyAsync();
        }

        public Task<double> GetPopularity(long postId)
        {
            var query = from p in DbContext.Posts
                let comments = (from c in DbContext.PostComments where c.PostId == p.Id select c).Count()
                let likes = (from l in DbContext.PostLikes where l.PostId == p.Id select l).Count()
                let timeDecay = Math.Exp(-(DateTime.Now - p.CreatedAt).TotalHours)
                let rank = (comments + likes * 2) * timeDecay
                select rank;
            return query.FirstOrDefaultAsync();
        }

        #region Map layer state

        public async Task<MapLayerState> GetLayerState(
            Polygon polygon,
            User? availableFor = null,
            int limit = PostServiceDefaults.PopularPostsLimit)
        {
            var popular = await this.GetPopularPosts(limit, polygon, availableFor: availableFor);
            return new MapLayerState
            {
                Popular = popular,
            };
        }

        #endregion

        #region Likes

        public async Task FavoritePost(MapPost post, string userId)
        {
            if (await DbContext.PostLikes.Where(l => l.UserId == userId && l.PostId == post.Id).AnyAsync())
                return;
            var like = new PostLike { UserId = userId, PostId = post.Id };
            DbContext.Add(like);
            await DbContext.SaveChangesAsync();
            await OnPopularityChange(post);
        }

        public async Task UnfavoritePost(MapPost post, string userId)
        {
            if (!await DbContext.PostLikes.Where(l => l.UserId == userId && l.PostId == post.Id).AnyAsync())
                return;
            await DbContext.PostLikes.Where(l => l.UserId == userId && l.PostId == post.Id).DeleteFromQueryAsync();
            await OnPopularityChange(post);
        }

        private async Task OnPopularityChange(MapPost post)
        {
            var likes = await DbContext.PostLikes.Where(l => l.PostId == post.Id).CountAsync();
            await _eventBus.Trigger(post.Id.ToString(), new PostEngagement
            {
                Likes = likes,
                Reposts = 0,
                Location = post.Location,
                PostId = post.Id
            });
        }

        #endregion
    }
}