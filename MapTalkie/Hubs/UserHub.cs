using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Models;
using MapTalkie.Services.EventBus;
using MapTalkie.Services.MessageService;
using MapTalkie.Services.MessageService.Events;
using MapTalkie.Services.PostService;
using MapTalkie.Services.PostService.Events;
using MapTalkie.Utils.MapUtils;
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

        private readonly IEventBus _eventBus;
        private readonly IPostService _postService;

        public MainUserHub(
            IPostService postService,
            IEventBus eventBus,
            UserManager<User> userManager) : base(userManager)
        {
            _eventBus = eventBus;
            _postService = postService;
        }

        public override Task OnConnectedAsync()
        {
            InitMessages();
            return Task.CompletedTask;
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            DisposeMessages();
            return Task.CompletedTask;
        }

        #region Messages

        private IDisposable? _messagesSubscription;
        private IDisposable? _conversationSubscription;
        private int? _activeConversationId;

        public async Task SetActiveConversation(int conversationId)
        {
            _activeConversationId = conversationId;
            _conversationSubscription = _eventBus.Subscribe<ConversationUpdate>(
                conversationId.ToString(), OnConversationUpdate);
        }

        private async Task OnConversationUpdate(ConversationUpdate @event)
        {
            await Clients.Caller.SendAsync("PM", @event);
        }


        public async Task RemoveActiveConversation()
        {
            _activeConversationId = null;
            _conversationSubscription?.Dispose();
        }

        private void InitMessages()
        {
            _messagesSubscription = _eventBus.Subscribe<MessageEvent>(
                MessageServiceDefaults.MessageEventPrefix + UserId,
                OnMessageEvent);
        }

        private void DisposeMessages()
        {
            _messagesSubscription?.Dispose();
            _conversationSubscription?.Dispose();
        }

        private async Task OnMessageEvent(MessageEvent @event)
        {
            await Clients.Caller.SendAsync("Message", @event);
        }

        #endregion

        #region Map

        private SubscriptionOptions? _subscriptionOptions;
        private IDisposable? _postUpdateSubscription;

        public class SubscriptionOptions
        {
            [Required] public Polygon ViewPort { get; set; } = default!;
            [Required] public SubscriptionType SubscriptionType { get; set; }
        }

        public async Task ConfigureSubscription(SubscriptionOptions options)
        {
            var old = _subscriptionOptions;
            _subscriptionOptions = options;

            if (old == null ||
                !MapUtils.IsSameArea(old.ViewPort, options.ViewPort) ||
                old.SubscriptionType != options.SubscriptionType)
            {
                await SendPostsInCurrentArea();
                await UpdateSubscriptions();
            }
        }

        private async Task UpdateSubscriptions()
        {
            if (_subscriptionOptions == null)
            {
                throw new InvalidOperationException("Can't update subscriptions since _subscriptionOptions is not set");
            }

            _postUpdateSubscription?.Dispose();

            switch (_subscriptionOptions.SubscriptionType)
            {
                case SubscriptionType.Latest:
                    _postUpdateSubscription = SubscribeToLatestPosts();
                    break;
                case SubscriptionType.Popular:
                    _postUpdateSubscription = SubscribeToPopularPosts();
                    break;
                default:
                    throw new InvalidOperationException("Invalid subscription type");
            }
        }

        private IDisposable SubscribeToPopularPosts()
        {
            if (_subscriptionOptions == null)
                throw new InvalidOperationException("Can't subscribe to area event: subscription options is not set");
            return _eventBus.Subscribe<PopularPostEvent>(
                _subscriptionOptions.ViewPort, string.Empty, @event => @event.Location, OnPopularPost);
        }

        private async Task OnPopularPost(PopularPostEvent @event)
        {
            await Clients.Caller.SendAsync("PopularPost", @event);
        }


        private IDisposable SubscribeToLatestPosts()
        {
            if (_subscriptionOptions == null)
                throw new InvalidOperationException("Can't subscribe to area event: subscription options is not set");
            return _eventBus.Subscribe<NewPostEvent>(
                _subscriptionOptions.ViewPort, string.Empty, @event => @event.Location, OnNewPost);
        }

        private async Task OnNewPost(NewPostEvent @event)
        {
            await Clients.Caller.SendAsync("NewPost", @event);
        }

        private async Task SendPostsInCurrentArea()
        {
            if (_subscriptionOptions == null)
            {
                throw new InvalidOperationException(
                    "Can't send posts in current area since _subscriptionOptions is not set");
            }

            IQueryable<Post> queryable;
            switch (_subscriptionOptions.SubscriptionType)
            {
                case SubscriptionType.Latest:
                    queryable = _postService.QueryPosts(_subscriptionOptions.ViewPort)
                        .OrderByDescending(p => p.CreatedAt);
                    break;
                case SubscriptionType.Popular:
                    queryable = _postService.QueryPopularPosts(_subscriptionOptions.ViewPort);
                    break;
                default:
                    throw new NotImplementedException();
            }

            var views = await queryable.Select(p => new
            {
                p.Id, p.Location, p.CreatedAt,
                p.UserId, UserName = p.User.UserName,
                Likes = p.Likes.Count,
                Reposts = 0,
                Comments = p.Comments.Count
            }).Take(100).ToListAsync();
            await Clients.Caller.SendAsync("OnPosts", views, _subscriptionOptions.SubscriptionType);
        }

        #endregion

        #region Engagement

        private readonly Dictionary<long, IDisposable> _engagementDisposables = new();

        public async Task TrackPosts(long[] postIds)
        {
            foreach (var postId in postIds)
            {
                SubscribeToEngagement(postId);
            }

            var postPops = await _postService
                .QueryPopularity(availableFor: await GetUser())
                .Where(p => postIds.Contains(p.PostId))
                .ToListAsync();
            await Clients.Caller.SendAsync("Engagements", postPops);
        }

        public Task StopTrackingPosts(long[] postIds)
        {
            foreach (var id in postIds)
            {
                UnsubscribeFromEngagement(id);
            }

            return Task.CompletedTask;
        }

        private void SubscribeToEngagement(long postId)
        {
            if (_engagementDisposables.ContainsKey(postId))
                return;
            if (_subscriptionOptions == null)
            {
                throw new InvalidOperationException(
                    "Can't subscribe to engagement update - _subscriptionOptions is not set");
            }

            _engagementDisposables[postId] = _postService.SubscribeToEngagement(postId, OnEngagement);
        }

        private void UnsubscribeFromEngagement(long postId)
        {
            if (_engagementDisposables.ContainsKey(postId))
            {
                _engagementDisposables[postId].Dispose();
                _engagementDisposables.Remove(postId);
            }
        }

        private async Task OnEngagement(PostEngagement engagement)
        {
            await Clients.Caller.SendAsync("Engagement", engagement.Popularity);
        }

        #endregion
    }
}