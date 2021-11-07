using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using MapTalkie.Services.EventBus;
using MapTalkie.Services.PostService;
using MapTalkie.Services.PostService.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NetTopologySuite.Geometries;

namespace MapTalkie.Hubs
{
    [Authorize]
    public class PostsHub : Hub
    {
        private readonly IEventBus _eventBus;
        private readonly IPostService _postService;

        private IDisposable? _engagementSubscription;
        private IDisposable? _postUpdateSubscription;

        public PostsHub(IPostService postService, IEventBus eventBus)
        {
            _postService = postService;
            _eventBus = eventBus;
        }

        public override async Task OnConnectedAsync()
        {
        }

        public async Task SetWatchArea(Polygon polygon)
        {
            _postUpdateSubscription?.Dispose();
            // TODO устновить ораничение на количество событий 
            _postUpdateSubscription = _eventBus.Subscribe<PostUpdate>(
                polygon, string.Empty, e => e.Point,
                async (engagement) => { await Clients.Caller.SendAsync("PostUpdate", engagement); });
            await Clients.Caller.SendAsync("NewWatchArea", polygon);
        }

        private void SubscribeToEngagement(long[] postIds)
        {
            _engagementSubscription?.Dispose();
            var disposables = postIds.Select(
                    id => _eventBus.Subscribe<PostEngagement>(id.ToString(), OnEngagement))
                .ToList();
            _engagementSubscription = Disposable.Create(() =>
            {
                foreach (var d in disposables)
                    d.Dispose();
            });
        }

        private async Task OnEngagement(PostEngagement engagement)
        {
        }
    }
}