using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.DB;
using MapTalkie.DB.Context;
using MapTalkie.Domain.Utils;
using MapTalkie.Services.PopularityProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;

namespace MapTalkie.Hubs
{
    [Authorize]
    public class UserHub : AuthorizedHub
    {
        public enum SubscriptionType
        {
            Latest,
            Popular
        }

        public static string DirectMessage = "Dm";
        public static string DirectMessageDeleted = "DmDeleted";
        public static string PostsUpdate = "Posts";
        public static string PostEngagement = "Engagement";

        private readonly AppDbContext _context;
        private readonly IPopularityProvider _popularityProvider;


        public UserHub(
            IPopularityProvider popularityProvider,
            AppDbContext context,
            UserManager<User> userManager) : base(userManager)
        {
            _context = context;
            _popularityProvider = popularityProvider;
        }

        #region PrivateMessages

        private int? _activeConversationId;

        public Task SetActiveConversation(int conversationId)
        {
            return SetActiveConversationImpl(conversationId);
        }

        public Task RemoveActiveConversation()
        {
            return SetActiveConversationImpl(null);
        }

        private async Task SetActiveConversationImpl(int? conversationId)
        {
            if (conversationId == _activeConversationId)
                return;

            if (_activeConversationId != null)
                await Groups.RemoveFromGroupAsync(Context.ConnectionId,
                    MapTalkieGroups.ConversationPrefix + _activeConversationId);

            _activeConversationId = conversationId;
            if (_activeConversationId != null)
                await Groups.AddToGroupAsync(Context.ConnectionId,
                    MapTalkieGroups.ConversationPrefix + _activeConversationId);
        }

        #endregion

        #region Map

        private SubscriptionType _subscriptionType = SubscriptionType.Popular;
        private Polygon? _subscriptionPolygon;

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
                    new Coordinate(northEast.X, northEast.Y)
                })) { SRID = 3857 };
            var oldPoly = _subscriptionPolygon;
            var oldType = _subscriptionType;
            _subscriptionPolygon = polygon;
            _subscriptionType = subscriptionType;

            if (oldPoly == null ||
                !AreaId.IsSameArea(oldPoly, polygon) ||
                subscriptionType != oldType)
            {
                if (oldPoly != null)
                    await Groups.RemoveFromGroupAsync(
                        Context.ConnectionId,
                        MapTalkieGroups.AreaUpdatesPrefix + _subscriptionType + AreaId.FromPolygon(oldPoly).Id);

                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    MapTalkieGroups.AreaUpdatesPrefix + _subscriptionType + AreaId.FromPolygon(polygon));
            }
        }

        #endregion

        #region Engagement

        private HashSet<long> _trackingPosts = new();

        public async Task TrackPosts(long[] postIds)
        {
            var set = new HashSet<long>(postIds);
            foreach (var postId in _trackingPosts.Where(postId => !set.Contains(postId)))
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, MapTalkieGroups.PostUpdatesPrefix + postId);

            foreach (var postId in set.Where(postId => !_trackingPosts.Contains(postId)))
                await Groups.AddToGroupAsync(Context.ConnectionId, MapTalkieGroups.PostUpdatesPrefix + postId);

            _trackingPosts = set;
        }

        public async Task StopTrackingPosts()
        {
            foreach (var postId in _trackingPosts)
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, MapTalkieGroups.PostUpdatesPrefix + postId);
        }

        #endregion

        #region Data types

        // TODO

        #endregion
    }
}