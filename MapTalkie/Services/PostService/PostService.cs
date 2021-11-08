using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MapTalkie.Models;
using MapTalkie.Models.Context;
using MapTalkie.Services.EventBus;
using MapTalkie.Services.PostService.Events;
using MapTalkie.Utils.MapUtils;
using MapTalkie.Utils.RTEFC;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NetTopologySuite.Geometries;

namespace MapTalkie.Services.PostService
{
    public class PostService : DbService, IPostService
    {
        private static readonly SemaphoreSlim RtefcSemaphore = new(0, 1);
        private static readonly SemaphoreSlim PopularityCacheUpdateSemaphore = new(0, 1);
        private readonly IEventBus _eventBus;
        private readonly IMemoryCache _memoryCache;

        public PostService(AppDbContext context, IEventBus eventBus, IMemoryCache memoryCache) : base(context)
        {
            _eventBus = eventBus;
            _memoryCache = memoryCache;
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
            await OnPostCreated(post);
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

        public IQueryable<Post> QueryPopularPosts(
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

        #region Map layer state

        public async Task<MapLayerState> GetLayerState(
            Polygon polygon,
            User? availableFor = null,
            bool byPassCache = false)
        {
            var zone = MapUtils.GetZone(polygon);
            List<Post> posts;

            if (byPassCache)
            {
                posts = await QueryPopularPosts(geometry: MapUtils.GetZonePolygon(zone)).ToListAsync();
            }
            else
            {
                var key = PostServiceDefaults.PopularPostsCacheKeyPrefix + zone.ToIdentifier();
                if (!_memoryCache.TryGetValue<CachedPopularityZoneEntry>(key, out var cached))
                {
                    await UpdatePopularityCache(zone);
                    cached = _memoryCache.Get<CachedPopularityZoneEntry>(key);
                }

                var postIds = cached.Posts.Select(p => p.PostId).ToArray();
                posts = await DbContext.Posts.Where(p => postIds.Contains(p.Id)).ToListAsync();
            }

            var clusters = await GetClustersForZone(zone);
            return new MapLayerState
            {
                Popular = posts,
                Clusters = clusters.Rtefc.Clusters.Select(c => new MapCluster
                {
                    Centroid = new Point(MapUtils.MercatorToLatLon(c.Centroid)) { SRID = 4326 },
                    ClusterId = c.Id.ToString(),
                    ClusterSize = c.Value
                }).ToList()
            };
        }

        private struct CachedRtefcEntry
        {
            public Rtefc Rtefc;
            public SemaphoreSlim Semaphore;
        }

        private async Task<CachedRtefcEntry> GetClustersForZone(MapZoneDescriptor zone)
        {
            var cacheKey = PostServiceDefaults.PostClustersCacheKeyPrefix + zone.ToIdentifier();
            if (!_memoryCache.TryGetValue<CachedRtefcEntry>(cacheKey, out var clusters))
            {
                var isNew = false;
                await RtefcSemaphore.WaitAsync();
                if (!_memoryCache.TryGetValue<CachedRtefcEntry>(cacheKey, out clusters))
                {
                    isNew = true;
                    clusters = new CachedRtefcEntry
                    {
                        Rtefc = new Rtefc(
                            TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(9),
                            50, MapUtils.ZoneSize(zone.Level)),
                        Semaphore = new(1, 1)
                    };
                }

                RtefcSemaphore.Release();
                if (isNew)
                {
                    // не оптимально
                    // TODO оптимизировать это дело
                    var posts = await QueryPosts(MapUtils.GetZonePolygon(zone))
                        .Where(p => p.CreatedAt > DateTime.UtcNow - TimeSpan.FromDays(14))
                        .OrderByDescending(p => p.CreatedAt)
                        .Select(p => p.Location)
                        .Take(30000)
                        .ToListAsync();
                    for (var i = 0; i < posts.Count; i++)
                        clusters.Rtefc.Add(posts[i].Coordinate, 1);
                    clusters.Semaphore.Release();
                }
            }

            return clusters;
        }

        private async Task OnPostCreated(Post post)
        {
            var zones = MapUtils.GetZones(post.Location);
            foreach (var zone in zones)
            {
                // TODO add in batches
                var clusters = await GetClustersForZone(zone);
                await clusters.Semaphore.WaitAsync();
                clusters.Rtefc.Add(post.Location.Coordinate, 1);
                clusters.Semaphore.Release();
            }
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

        public async Task UnfavoritePost(Post post, string userId)
        {
            if (!await DbContext.PostLikes.Where(l => l.UserId == userId && l.PostId == post.Id).AnyAsync())
                return;
            await DbContext.PostLikes.Where(l => l.UserId == userId && l.PostId == post.Id).DeleteFromQueryAsync();
            await OnPopularityChange(post);
        }

        #endregion

        #region Все связанное с популярностью

        private async Task OnPopularityChange(Post post)
        {
            var popularity = await GetPopularity(post.Id);
            await _eventBus.Trigger(post.Id.ToString(), new PostEngagement
            {
                Popularity = popularity,
                Location = post.Location,
                PostId = post.Id
            });

            await AdjustPopularityCache(post, popularity);
        }

        private struct CachedPopularPost
        {
            public long PostId { get; set; }
            public DateTime CreatedAt { get; set; }
            public double FreshRank { get; set; }

            public double Rank => FreshRank * CalculateDecay(CreatedAt);
        }

        private class CachedPopularityZoneEntry
        {
            public List<CachedPopularPost> Posts { get; set; }
            public SemaphoreSlim Semaphore { get; set; }
        }

        private async Task AdjustPopularityCache(Post post, PostPopularity popularity)
        {
            var zones = MapUtils.GetZones(post.Location);

            var tasks = new List<Task>();

            foreach (var zone in zones)
            {
                if (!_memoryCache.TryGetValue<CachedPopularityZoneEntry>(
                    PostServiceDefaults.PopularPostsCacheKeyPrefix + zone.ToIdentifier(), out var popular))
                {
                    tasks.Add(UpdatePopularityCache(zone));
                }
                else
                {
                    var index = popular.Posts.FindIndex(cached => cached.Rank < popularity.Rank);
                    if (index != -1)
                    {
                        async Task Add()
                        {
                            await popular.Semaphore.WaitAsync();

                            popular.Posts.Insert(index, new CachedPopularPost
                            {
                                PostId = post.Id,
                                CreatedAt = post.CreatedAt,
                                FreshRank = popularity.FreshRank
                            });
                            popular.Posts.RemoveAt(popular.Posts.Count - 1);
                            popular.Semaphore.Release();

                            await _eventBus.Trigger(
                                PostServiceDefaults.PopularPostsUpdatedInAreaEventPrefix + zone.ToIdentifier(),
                                new PopularPostsUpdated { ZoneDescriptor = zone });
                        }

                        tasks.Add(Add());
                    }
                }

                await Task.WhenAll(tasks);
            }
        }

        private async Task UpdatePopularityCache(MapZoneDescriptor zone)
        {
            await PopularityCacheUpdateSemaphore.WaitAsync();

            try
            {
                var key = PostServiceDefaults.PopularPostsCacheKeyPrefix + zone.ToIdentifier();
                if (!_memoryCache.TryGetValue(key, out var _))
                {
                    var popularPosts = await QueryPopularPostsImpl(100, MapUtils.GetZonePolygon(zone))
                        .Select(pair => new CachedPopularPost
                        {
                            PostId = pair.Post.Id,
                            CreatedAt = pair.Post.CreatedAt,
                            FreshRank = pair.Popularity.FreshRank
                        })
                        .ToListAsync();
                    _memoryCache.Set(key,
                        new CachedPopularityZoneEntry { Posts = popularPosts, Semaphore = new(0, 1) });
                }
            }
            finally
            {
                PopularityCacheUpdateSemaphore.Release();
            }
        }

        public Task<PostPopularity> GetPopularity(long postId)
        {
            var query = from p in DbContext.Posts
                let comments = (from c in DbContext.PostComments where c.PostId == p.Id select c).Count()
                let likes = (from l in DbContext.PostLikes where l.PostId == p.Id select l).Count()
                let timeDecay = CalculateDecay(p.CreatedAt)
                let freshRank = comments + likes * 2
                let rank = (comments + likes * 2) * timeDecay
                select new PostPopularity
                {
                    Likes = likes,
                    Reposts = 0,
                    Rank = rank,
                    FreshRank = freshRank,
                    TimeDecayFactor = timeDecay,
                    Comments = comments
                };
            return query.FirstOrDefaultAsync();
        }

        private static double CalculateDecay(DateTime dt) => Math.Exp(-(DateTime.Now - dt).TotalHours);

        #endregion
    }
}