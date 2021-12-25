using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.DB;
using MapTalkie.DB.Context;
using MapTalkie.Domain.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;

namespace MapTalkie.Hubs;

[Authorize]
public class UserHub : AuthorizedHub
{
    public static string DirectMessage = "Dm";
    public static string DirectMessageDeleted = "DmDeleted";
    public static string PostsUpdate = "Posts";
    public static string PostEngagement = "Engagement";

    private static readonly ConcurrentDictionary<string, StateType> ClientStates = new();

    private readonly AppDbContext _context;

    public UserHub(
        AppDbContext context,
        UserManager<User> userManager) : base(userManager)
    {
        _context = context;
    }

    private StateType State
    {
        get => ClientStates[Context.ConnectionId];
        set => ClientStates[Context.ConnectionId] = value;
    }

    public override async Task OnConnectedAsync()
    {
        ClientStates[Context.ConnectionId] = new StateType();
        await Groups.AddToGroupAsync(Context.ConnectionId, MapTalkieGroups.Messages + UserId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await SetActiveConversation(null);
        await DisableSubscription();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, MapTalkieGroups.Messages + UserId);
        ClientStates.Remove(Context.ConnectionId, out var _);
    }


    #region PrivateMessages

    public async Task SetActiveConversation(string? userId)
    {
        if (userId == State.ActiveConversation)
            return;

        if (State.ActiveConversation != null)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId,
                MapTalkieGroups.ConversationPrefix + State.ActiveConversation);

        State.ActiveConversation = userId;
        if (userId != null)
            await Groups.AddToGroupAsync(Context.ConnectionId,
                MapTalkieGroups.ConversationPrefix + userId);
    }

    #endregion

    private class StateType
    {
        public string? ActiveConversation { get; set; }
        public Polygon? SubscriptionPolygon { get; set; }
        public HashSet<long> TrackingPosts { get; set; } = new();
    }

    #region Map

    public async Task ConfigureSubscription(Point northEast, Point southWest)
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
        var oldPoly = State.SubscriptionPolygon;
        State.SubscriptionPolygon = polygon;

        if (oldPoly == null ||
            !AreaId.IsSameArea(oldPoly, polygon))
        {
            if (oldPoly != null)
                await Groups.RemoveFromGroupAsync(
                    Context.ConnectionId,
                    MapTalkieGroups.AreaUpdatesPrefix + AreaId.FromPolygon(oldPoly));

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                MapTalkieGroups.AreaUpdatesPrefix + AreaId.FromPolygon(polygon));
        }
    }

    public async Task DisableSubscription()
    {
        var state = ClientStates[Context.ConnectionId];
        if (state.SubscriptionPolygon != null)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                MapTalkieGroups.AreaUpdatesPrefix + AreaId.FromPolygon(state.SubscriptionPolygon));
            state.SubscriptionPolygon = null;
        }
    }

    #endregion

    #region Engagement

    public async Task TrackPosts(long[] postIds)
    {
        var state = ClientStates[Context.ConnectionId];
        var set = new HashSet<long>(postIds);
        var trackingPosts = State.TrackingPosts;
        foreach (var postId in trackingPosts.Where(postId => !set.Contains(postId)))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, MapTalkieGroups.PostUpdatesPrefix + postId);

        foreach (var postId in set.Where(postId => !trackingPosts.Contains(postId)))
            await Groups.AddToGroupAsync(Context.ConnectionId, MapTalkieGroups.PostUpdatesPrefix + postId);

        State.TrackingPosts = set;
    }

    public async Task StopTrackingPosts()
    {
        foreach (var postId in State.TrackingPosts)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, MapTalkieGroups.PostUpdatesPrefix + postId);
        State.TrackingPosts.Clear();
    }

    #endregion

    #region Data types

    // TODO

    #endregion
}