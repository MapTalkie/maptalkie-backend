using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Services.PostService;
using MapTalkieCommon.Utils;
using MapTalkieDB;
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

        public async Task ConfigureSubscription(Polygon polygon, SubscriptionType subscriptionType)
        {
            var oldPoly = _subscriptionPolygon;
            var oldType = _subscriptionType;
            _subscriptionPolygon = polygon;
            _subscriptionType = subscriptionType;

            if (oldPoly == null ||
                !ZoneId.IsSameArea(oldPoly, polygon) ||
                subscriptionType != oldType)
            {
                if (oldPoly != null)
                {
                    await Groups.RemoveFromGroupAsync(
                        Context.ConnectionId,
                        MapTalkieGroups.AreaUpdatesPrefix + _subscriptionType + ZoneId.FromPolygon(oldPoly).Id);
                }

                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    MapTalkieGroups.AreaUpdatesPrefix + _subscriptionType + ZoneId.FromPolygon(polygon));

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
            await Clients.Caller.SendAsync("Posts", views, _subscriptionType);
        }

        #endregion

        #region Engagement

        private HashSet<string> _trackingPosts = new();

        public async Task TrackPosts(string[] postIds)
        {
            var set = new HashSet<string>(postIds);
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