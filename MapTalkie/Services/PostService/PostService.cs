using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MapTalkie.Configuration;
using MapTalkie.Models;
using MapTalkie.Models.Context;
using MapTalkie.Services.EventBus;
using MapTalkie.Services.PostService.Events;
using MapTalkie.Utils.MapUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;

namespace MapTalkie.Services.PostService
{
    public class PostService : DbService, IPostService
    {
        private readonly IEventBus _eventBus;
        private readonly IMemoryCache _memoryCache;
        private readonly IOptions<PostOptions> _postOptions;

        public PostService(
            AppDbContext context,
            IEventBus eventBus,
            IMemoryCache memoryCache,
            IOptions<PostOptions> postOptions) : base(context)
        {
            _eventBus = eventBus;
            _memoryCache = memoryCache;
            _postOptions = postOptions;
        }

        #region CRUD things

        public async Task<Post> CreateTextPost(
            string text,
            string userId,
            Point location,
            bool isOriginalLocation)
        {
            var post = new Post
            {
                Text = text,
                Location = location,
                UserId = userId,
                IsOriginalLocation = isOriginalLocation
            };
            DbContext.Posts.Add(post);
            await DbContext.SaveChangesAsync();
            //await OnPostCreated(post);
            return post;
        }

        public Task<Post?> GetPostOrNull(long id, bool includeUnavailable)
        {
            var query = DbContext.Posts.Where(m => m.Id == id);
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

        public IQueryable<dynamic> QueryPopularPostViews(PostView view, Geometry? geometry = null,
            bool? available = true, User? availableFor = null)
        {
            return MakePostViewQuery(QueryPopularPosts(geometry, available, availableFor), view);
        }

        private IQueryable<object> MakePostViewQuery(IQueryable<Post> query, PostView view)
        {
            IQueryable<dynamic> viewQuery = query;

            switch (view)
            {
                case PostView.Minimal:
                    viewQuery = query.Select(p => new
                    {
                        p.Id, p.CreatedAt, p.UserId, p.User.UserName,
                        p.Location, p.IsOriginalLocation, p.UpdatedAt,
                        Likes = p.Likes.Count,
                        Reposts = 0,
                        Comments = p.Comments.Count
                    });
                    break;
                case PostView.Full:
                    viewQuery = query.Select(p => new
                    {
                        p.Id, p.CreatedAt, p.UserId, p.User.UserName,
                        p.Location, p.IsOriginalLocation, p.UpdatedAt, p.Text,
                        Likes = p.Likes.Count,
                        Reposts = 0,
                        Comments = p.Comments.Count
                    });
                    break;
            }

            return viewQuery;
        }


        private struct PopularPost
        {
            public Post Post { get; set; }
            public PostPopularity Popularity { get; set; }
        }

        private IQueryable<PopularPost> QueryPopularPostsImpl(
            int limit = PostServiceDefaults.PopularPostsLimit,
            Geometry? geometry = null,
            bool? available = true,
            User? availableFor = null)
        {
            var query = from p in QueryPosts(geometry, available, availableFor)
                let comments = (from c in DbContext.PostComments where c.PostId == p.Id select c).Count()
                let likes = (from l in DbContext.PostLikes where l.PostId == p.Id select l).Count()
                let timeDecay = CalculateDecay(p.CreatedAt)
                let freshRank = comments + likes * 2
                let rank = timeDecay * freshRank
                orderby rank descending
                select new PopularPost
                {
                    Popularity = new PostPopularity
                        { Rank = rank, FreshRank = freshRank, Comments = comments, Likes = likes, Reposts = 0 },
                    Post = p
                };

            return query.Take(limit);
        }

        public Task<bool> IsAvailable(long id)
        {
            return DbContext.Posts.Where(p => p.Id == id && p.Available).AnyAsync();
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
            await OnPopularityChange(post);
        }

        public async Task UnFavoritePost(Post post, string userId)
        {
            if (!await DbContext.PostLikes.Where(l => l.UserId == userId && l.PostId == post.Id).AnyAsync())
                return;
            await DbContext.PostLikes.Where(l => l.UserId == userId && l.PostId == post.Id).DeleteFromQueryAsync();
            await OnPopularityChange(post);
        }

        #endregion

        #region Все связанное с популярностью

        public IQueryable<Post> QueryPopularPosts(
            Geometry? geometry = null,
            bool? available = true,
            User? availableFor = null)
        {
            var query = from p in QueryPosts(geometry, available, availableFor)
                let comments = p.Comments.Count
                let likes = p.Likes.Count
                let timeDecay = Math.Exp(-(DateTime.Now - p.CreatedAt).TotalHours)
                let freshRank = comments + likes * 2
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
                let comments = p.Comments.Count
                let likes = p.Likes.Count
                let timeDecay = Math.Exp(-(DateTime.Now - p.CreatedAt).TotalHours)
                let freshRank = comments + likes * 2
                let rank = freshRank * timeDecay
                orderby rank descending
                select new PostPopularity
                {
                    FreshRank = freshRank,
                    Rank = rank,
                    Likes = likes,
                    Comments = comments,
                    PostId = p.Id,
                    TimeDecayFactor = timeDecay,
                    Reposts = 0
                };
        }

        public IDisposable SubscribeToEngagement(long postId, Func<PostEngagement, Task> callback)
            => _eventBus.Subscribe(postId, callback);

        public IDisposable SubscribeToEngagement(Polygon polygon, Func<PostEngagement, Task> callback)
            => _eventBus.Subscribe(polygon, string.Empty, @event => @event.Location, callback);

        private async Task OnPopularityChange(Post post)
        {
            var popularity = await GetPopularity(post.Id);
            await _eventBus.Trigger(post.Id, new PostEngagement
            {
                Popularity = popularity,
                Location = post.Location,
            });

            await AdjustPopularityCache(post);
        }

        private class PopularityCacheEntry
        {
            public bool Exists { get; set; }
            public long PostId { get; set; }
            public double FreshRank { get; set; }
            public DateTime CreatedAt { get; set; }
            public SemaphoreSlim Semaphore { get; } = new(1, 1);
            public double Rank => CalculateDecay(CreatedAt) * Rank;
        }

        private static readonly SemaphoreSlim PopularityCacheSemaphore = new(1, 1);

        private async Task AdjustPopularityCache(Post post)
        {
            var popularity = await GetPopularity(post.Id);
            foreach (var zoneDescriptor in MapUtils.GetZones(post.Location))
            {
                var key = PostServiceDefaults.PopularPostsInAreaKey[zoneDescriptor.ToIdentifier()];
                if (!_memoryCache.TryGetValue<PopularityCacheEntry>(key, out var cacheEntry))
                {
                    await PopularityCacheSemaphore.WaitAsync();
                    if (!_memoryCache.TryGetValue<PopularityCacheEntry>(key, out cacheEntry))
                    {
                        // инициализировать запись кэша поместив в него N-ый популярный пост в указанной зоне
                        var leastPopularFromTop = await
                            QueryPopularPosts(MapUtils.GetZonePolygon(zoneDescriptor))
                                .Skip(_postOptions.Value.PopularPostsCachedTopCount - 1)
                                .FirstOrDefaultAsync();
                        if (leastPopularFromTop == null)
                        {
                            cacheEntry = new PopularityCacheEntry { Exists = false };
                        }
                        else
                        {
                            cacheEntry = new PopularityCacheEntry
                            {
                                Exists = true,
                                PostId = post.Id,
                                CreatedAt = post.CreatedAt,
                                FreshRank = popularity.FreshRank
                            };
                        }

                        _memoryCache.Set(key, cacheEntry);
                    }
                    else
                    {
                        // запись кэша обновилась пока мы ждали семафор
                        if (cacheEntry.PostId != post.Id && cacheEntry.Rank < popularity.Rank)
                        {
                            cacheEntry.PostId = post.Id;
                            cacheEntry.FreshRank = popularity.FreshRank;
                            cacheEntry.CreatedAt = post.CreatedAt;
                        }
                    }

                    PopularityCacheSemaphore.Release();
                }
                else
                {
                    if (cacheEntry.PostId != post.Id && cacheEntry.Rank < popularity.Rank)
                    {
                        await cacheEntry.Semaphore.WaitAsync();
                        if (cacheEntry.PostId != post.Id && cacheEntry.Rank < popularity.Rank)
                        {
                            cacheEntry.PostId = post.Id;
                            cacheEntry.FreshRank = popularity.FreshRank;
                            cacheEntry.CreatedAt = post.CreatedAt;
                        }

                        cacheEntry.Semaphore.Release();
                    }
                }
            }
        }

        public Task<PostPopularity> GetPopularity(long postId)
        {
            return QueryPopularity(available: null).Where(p => p.PostId == postId).FirstOrDefaultAsync();
        }

        private static double CalculateDecay(DateTime dt) => Math.Exp(-(DateTime.Now - dt).TotalHours);

        #endregion
    }
}