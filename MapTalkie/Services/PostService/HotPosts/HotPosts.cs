using System.Collections.Generic;
using System.Threading.Tasks;
using MapTalkie.Jobs;
using MapTalkie.Models;
using MapTalkie.Utils.MapUtils;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Quartz;

namespace MapTalkie.Services.PostService.HotPosts
{
    public class HotPosts : IHotPosts
    {
        private readonly IPostService _postService;
        private readonly ISchedulerFactory _schedulerFactory;

        public HotPosts(ISchedulerFactory factory, IPostService postService)
        {
            _schedulerFactory = factory;
            _postService = postService;
        }

        public Task ReportPostPopularityChange(MapPost post)
        {
            return ReportPostPopularityChangeImpl(post.Id, MapUtils.LatLonToMercator(post.Location));
        }

        public async Task UpdateZone(MapZoneDescriptor zoneDescriptor)
        {
            var posts = await _postService.QueryPopularPosts(
                50, MapUtils.GetAreaPolygon(zoneDescriptor)).ToListAsync();
        }

        private async Task ReportPostPopularityChangeImpl(long postId, Point postLocation)
        {
            MapUtils.ThrowIfNot3857(postLocation);
            var zonesForUpdate = MapUtils.GetZones(postLocation);
            await TriggerZoneUpdate(zonesForUpdate);
        }

        private async Task TriggerZoneUpdate(List<MapZoneDescriptor> zones)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var trigger = TriggerBuilder.Create().StartNow().Build();
            var job = FetchPopularPostsJob
                .Builder(new FetchPopularPostsData { Areas = zones })
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }
}