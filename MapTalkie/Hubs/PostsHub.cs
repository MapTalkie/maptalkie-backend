using System;
using System.Threading.Tasks;
using MapTalkie.Services.PostService;
using MapTalkie.Utils.EventBus;
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
            Console.WriteLine(polygon);
            //var coords = polygon.Boundary.Coordinates;
        }
    }
}