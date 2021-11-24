using System;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Common.Messages.Posts;
using MapTalkie.Common.Popularity;
using MapTalkie.Common.Utils;
using MapTalkie.DB;
using MapTalkie.DB.Context;
using MapTalkie.MessagesImpl;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace MapTalkie.Services.PostService
{
    public class PostService : DbService, IPostService
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public PostService(
            AppDbContext context,
            IPublishEndpoint publishEndpoint) : base(context)
        {
            _publishEndpoint = publishEndpoint;
        }

        #region CRUD things

        public async Task DeletePost(Post post)
        {
            post.Available = false;
            await DbContext.SaveChangesAsync();
            await _publishEndpoint.Publish<IPostDeleted>(new PostDeletedEvent
            {
                PostId = post.Id,
                LocationDescriptor = (LocationDescriptor)post.Location
            });
        }

        public async Task<Post> CreateTextPost(
            string text,
            string userId,
            Point location,
            bool isOriginalLocation)
        {
            var post = new Post
            {
                Text = text,
                Location = MapConvert.ToMercator(location),
                UserId = userId,
                IsOriginalLocation = isOriginalLocation
            };
            DbContext.Posts.Add(post);
            await DbContext.SaveChangesAsync();
            await _publishEndpoint.Publish<IPostCreated>(new PostCreatedEvent
            {
                PostId = post.Id,
                Location = (LocationDescriptor)post.Location
            });
            return post;
        }

        public Task<Post?> GetPostOrNull(long postId, bool includeUnavailable)
        {
            var query = DbContext.Posts.Where(m => m.Id == postId);
            if (!includeUnavailable)
                query = query.Where(p => p.Available);
            return query.FirstOrDefaultAsync()!;
        }

        public IQueryable<Post> QueryPosts(
            Geometry? geometry = null,
            bool? available = true,
            User? availableFor = null)
        {
            IQueryable<Post> query = DbContext.Posts;
            if (available != null)
                query = query.Where(p => p.Available == available);
            if (geometry != null)
                query = DbContext.Posts.Where(m => geometry.Contains(m.Location));

            if (availableFor != null)
                query = from p in query
                    join b in DbContext.BlacklistedUsers
                        on p.UserId equals b.UserId into bs
                    from b in bs.DefaultIfEmpty()
                    where b.BlacklistedById != availableFor.Id
                    select p;

            return query;
        }

        public Task<bool> IsAvailable(long postId)
        {
            return DbContext.Posts.Where(p => p.Id == postId && p.Available).AnyAsync();
        }

        #endregion

        #region Likes

        public async Task FavoritePost(Post post, string userId)
        {
            if (await DbContext.PostLikes.Where(l => l.UserId == userId && l.PostId == post.Id).AnyAsync())
                return;
            var like = new PostLike { UserId = userId, PostId = post.Id };
            DbContext.Add(like);
            await DbContext.SaveChangesAsync();
            await _publishEndpoint.Publish<IPostEngagement>(new PostEngagementEvent
            {
                PostId = post.Id,
                UserId = userId,
                Type = PostEngagementType.Favorite
            });
        }

        public async Task UnFavoritePost(Post post, string userId)
        {
            if (!await DbContext.PostLikes.Where(l => l.UserId == userId && l.PostId == post.Id).AnyAsync())
                return;
            await DbContext.PostLikes.Where(l => l.UserId == userId && l.PostId == post.Id).DeleteFromQueryAsync();
            await _publishEndpoint.Publish<IPostEngagement>(new PostEngagementEvent
            {
                PostId = post.Id,
                UserId = userId,
                Type = PostEngagementType.FavoriteRemoved
            });
        }

        #endregion

        #region Все связанное с популярностью

        public IQueryable<Post> QueryPopularPosts(
            Geometry? geometry = null,
            bool? available = true,
            User? availableFor = null)
        {
            var query = from p in QueryPosts(geometry, available, availableFor)
                let comments = p.Comments.Count(c => c.CreatedAt > p.CacheUpdatedAt) + p.CachedCommentsCount
                let likes = p.Likes.Count(c => c.CreatedAt > p.CacheUpdatedAt) + p.CachedLikesCount
                let shares = p.Shares.Count(c => c.CreatedAt > p.CacheUpdatedAt) + p.CachedSharesCount
                let interval = DateTime.UtcNow - p.CreatedAt
                let timeDecay = Math.Exp(-(interval.Days * 24 + interval.Hours))
                let freshRank =
                    comments * PopularityConstants.CommentsMultiplier +
                    likes * PopularityConstants.LikesMultiplier +
                    shares * PopularityConstants.SharesMultiplier
                let rank = freshRank * timeDecay
                orderby rank descending
                select p;

            return query;
        }

        public IQueryable<PostPopularity> QueryPopularity(
            Geometry? geometry = null,
            bool? available = true,
            User? availableFor = null)
        {
            return from p in QueryPosts(geometry, available, availableFor)
                let comments = p.Comments.Count(c => c.CreatedAt > p.CacheUpdatedAt) + p.CachedCommentsCount
                let likes = p.Likes.Count(c => c.CreatedAt > p.CacheUpdatedAt) + p.CachedLikesCount
                let shares = p.Shares.Count(c => c.CreatedAt > p.CacheUpdatedAt) + p.CachedSharesCount
                let interval = DateTime.UtcNow - p.CreatedAt
                let timeDecay = Math.Exp(-(interval.Days * 24 + interval.Hours))
                let freshRank = comments + likes * 2
                let rank = (p.CachedFreshRank + freshRank) * timeDecay
                orderby rank descending
                select new PostPopularity
                {
                    FreshRank = freshRank,
                    Rank = rank,
                    Likes = likes,
                    Comments = comments,
                    PostId = p.Id,
                    TimeDecayFactor = timeDecay,
                    Shares = 0
                };
        }

        public IQueryable<Post> QueryByComments(
            Geometry? geometry = null,
            User? availableFor = null)
        {
            return from p in QueryPosts(geometry, availableFor: availableFor)
                let comments = p.CachedCommentsCount + p.Comments.Count(c => c.CreatedAt > p.CacheUpdatedAt)
                orderby comments descending
                select p;
        }

        public IQueryable<Post> QueryByLikes(
            Geometry? geometry = null,
            User? availableFor = null)
        {
            return from p in QueryPosts(geometry, availableFor: availableFor)
                let likes = p.CachedLikesCount + p.Likes.Count(c => c.CreatedAt > p.CacheUpdatedAt)
                orderby likes descending
                select p;
        }

        public IQueryable<Post> QueryByShares(
            Geometry? geometry = null,
            User? availableFor = null)
        {
            return from p in QueryPosts(geometry, availableFor: availableFor)
                let shares = p.CachedSharesCount + p.Shares.Count(c => c.CreatedAt > p.CacheUpdatedAt)
                orderby shares descending
                select p;
        }

        public Task<PostPopularity> GetPopularity(long postId)
        {
            return QueryPopularity(available: null).Where(p => p.PostId == postId).FirstOrDefaultAsync();
        }

        #endregion
    }
}