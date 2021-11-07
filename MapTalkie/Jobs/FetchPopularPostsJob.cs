using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Services.EventBus;
using MapTalkie.Services.PostService;
using MapTalkie.Utils.MapUtils;
using Microsoft.Extensions.Caching.Memory;
using Quartz;

namespace MapTalkie.Jobs
{
    public class FetchPostsEntry
    {
        public MapZoneDescriptor Descriptor { get; set; }
        [Required] public string CacheKey { get; set; } = default!;
    }

    public class FetchPopularPostsData
    {
        public List<FetchPostsEntry> Areas { get; set; } = new();
    }

    public class FetchPopularPostsJob : JsonJob<FetchPopularPostsData>
    {
        private readonly IMemoryCache _cache;
        private readonly IEventBus _eventBus;
        private readonly IPostService _postService;

        public FetchPopularPostsJob(IPostService postService, IMemoryCache cache, IEventBus eventBus)
        {
            _postService = postService;
            _cache = cache;
            _eventBus = eventBus;
        }

        protected override async Task Execute(FetchPopularPostsData data, IJobExecutionContext context)
        {
            await Task.WhenAll(data.Areas.Select(UpdateZone));
            await _eventBus.Trigger(data);
        }

        private async Task UpdateZone(FetchPostsEntry update)
        {
            var posts = await _postService.GetPopularPosts(geometry: MapUtils.GetAreaPolygon(update.Descriptor));
            _cache.Set(update.CacheKey, posts, TimeSpan.FromDays(1));
        }
    }
}