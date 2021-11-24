using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Common.Utils;
using MapTalkie.DB;
using MapTalkie.Services.PostService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace MapTalkie.Hubs
{
    [Authorize]
    public class MainUserHub : AuthorizedHub
    {
        public enum SubscriptionType
        {
            Latest,
            Popular
        }

        private readonly IPostService _postService;

        public MainUserHub(
            IPostService postService,
            UserManager<User> userManager) : base(userManager)
        {
            _postService = postService;
        }

        #region Messages

        private int? _activeConversationId;

        public Task SetActiveConversation(int conversationId)
            => SetActiveConversationImpl(conversationId);

        public Task RemoveActiveConversation()
            => SetActiveConversationImpl(null);

        private async Task SetActiveConversationImpl(int? conversationId)
        {
            if (conversationId == _activeConversationId)
                return;

            if (_activeConversationId != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId,
                    MapTalkieGroups.ConversationPrefix + _activeConversationId);
            }

            _activeConversationId = conversationId;
            if (_activeConversationId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId,
                    MapTalkieGroups.ConversationPrefix + _activeConversationId);
            }
        }

        #endregion

        #region Map

        private SubscriptionType _subscriptionType = SubscriptionType.Popular;
        private Polygon? _subscriptionPolygon;

        public class SubscriptionOptions
        {
            [Required] public Polygon ViewPort { get; set; } = default!;
            [Required] public SubscriptionType SubscriptionType { get; set; }
        }

        public Task SetViewPort(Point northEast, Point southEast) =>
            ConfigureSubscription(northEast, southEast, _subscriptionType);

        public Task SetSubscriptionType(SubscriptionType subscriptionType) =>
            ConfigureSubscription(northEast, southEast, _subscriptionType);

        public async Task ConfigureSubscription(Point northEast, Point southWest, SubscriptionType subscriptionType)
        {
            northEast = MapConvert.ToMercator(northEast);
            southWest = MapConvert.ToMercator(southWest);
            var polygon = new Polygon(new LinearRing(
                new[]
                {
                    new Coordinate(northEast.X, northEast.Y),
                    new Coordinate(northEast.X, southWest.Y),
                    new Coordinate(southWest.X, southWest.Y),
                    new Coordinate(southWest.X, northEast.Y),
                    new Coordinate(northEast.X, northEast.Y),
                }));
            var oldPoly = _subscriptionPolygon;
            var oldType = _subscriptionType;
            _subscriptionPolygon = polygon;
            _subscriptionType = subscriptionType;

            if (oldPoly == null ||
                !AreaId.IsSameArea(oldPoly, polygon) ||
                subscriptionType != oldType)
            {
                if (oldPoly != null)
                {
                    await Groups.RemoveFromGroupAsync(
                        Context.ConnectionId,
                        MapTalkieGroups.AreaUpdatesPrefix + _subscriptionType + AreaId.FromPolygon(oldPoly).Id);
                }

                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    MapTalkieGroups.AreaUpdatesPrefix + _subscriptionType + AreaId.FromPolygon(polygon));

                await SendPostsInCurrentArea();
            }
        }

        private async Task SendPostsInCurrentArea()
        {
            IQueryable<Post> queryable;
            switch (_subscriptionType)
            {
                case SubscriptionType.Latest:
                    queryable = _postService.QueryPosts(_subscriptionPolygon)
                        .OrderByDescending(p => p.CreatedAt);
                    break;
                case SubscriptionType.Popular:
                    queryable = _postService.QueryPopularPosts(_subscriptionPolygon);
                    break;
                default:
                    throw new NotImplementedException();
            }

            var views = await queryable.Select(p => new
            {
                p.Id, p.Location, p.CreatedAt,
                p.UserId,
                p.User.UserName,
                Likes = p.Likes.Count,
                Shares = 0,
                Comments = p.Comments.Count
            }).Take(100).ToListAsync();
            await Clients.Caller.SendAsync("Posts", views, _subscriptionPolygon, _subscriptionType);
        }

        #endregion

        #region Engagement

        private HashSet<long> _trackingPosts = new();

        public async Task TrackPosts(long[] postIds)
        {
            var set = new HashSet<long>(postIds);
            foreach (var postId in _trackingPosts.Where(postId => !set.Contains(postId)))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, MapTalkieGroups.PostUpdatesPrefix + postId);
            }

            foreach (var postId in set.Where(postId => !_trackingPosts.Contains(postId)))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, MapTalkieGroups.PostUpdatesPrefix + postId);
            }

            _trackingPosts = set;

            var postPops = await _postService
                .QueryPopularity(availableFor: await GetUser())
                .Where(p => postIds.Contains(p.PostId))
                .ToListAsync();
            await Clients.Caller.SendAsync("Engagements", postPops);
        }

        public async Task StopTrackingPosts()
        {
            foreach (var postId in _trackingPosts)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, MapTalkieGroups.PostUpdatesPrefix + postId);
            }
        }

        #endregion
    }
}