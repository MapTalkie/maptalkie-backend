using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.DB;
using MapTalkie.DB.Context;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Services.FriendshipService;

public class FriendshipService : DbService, IFriendshipService
{
    public FriendshipService(AppDbContext context) : base(context)
    {
    }

    public async Task EnsureFriendshipRequest(string fromId, string toId)
    {
        var fr = await GetFriendshipRecord(fromId, toId);
        if (fr == null)
        {
            fr = new FriendRequest
            {
                FromId = fromId,
                ToId = toId
            };
            DbContext.FriendRequests.Add(fr);
            await DbContext.SaveChangesAsync();
        }
    }

    public async Task RevokeFriendship(string fromId, string toId)
    {
        var fr = await GetFriendshipRecord(fromId, toId);
        if (fr != null)
        {
            DbContext.FriendRequests.Remove(fr);
            await DbContext.SaveChangesAsync();
        }
    }

    public async Task<FriendshipsView> FindFriendships(string userId)
    {
        var friendRequests = await DbContext.FriendRequests
            .Where(r => r.FromId == userId || r.ToId == userId)
            .Select(r => new { r.FromId, r.ToId, To = r.To.UserName, From = r.From.UserName })
            .ToListAsync();

        var requestMutual = new Dictionary<string, byte>();
        var userView = new Dictionary<string, FriendView>();

        foreach (var friendRequest in friendRequests)
        {
            string id, username;
            var isOutgoingRequest = friendRequest.FromId == userId;
            if (isOutgoingRequest)
                (id, username) = (friendRequest.ToId, friendRequest.To);
            else
                (id, username) = (friendRequest.FromId, friendRequest.From);

            if (!userView.ContainsKey(id))
            {
                requestMutual[id] = 0;
                userView[id] = new FriendView(id, username);
            }

            requestMutual[id] |= (byte)(isOutgoingRequest ? 0b01 : 0b10);
        }

        var friends = new List<FriendView>();
        var incomingRequests = new List<FriendView>();
        var pendingRequests = new List<FriendView>();

        foreach (var kv in requestMutual)
            if (kv.Value == 3)
                friends.Add(userView[kv.Key]);
            else if (kv.Value == 2)
                incomingRequests.Add(userView[kv.Key]);
            else
                pendingRequests.Add(userView[kv.Key]);

        return new FriendshipsView(friends, pendingRequests, incomingRequests);
    }

    public async Task<FriendshipState> GetFriendshipState(string userId1, string userId2)
    {
        var requests = await DbContext.FriendRequests
            .Where(r => r.FromId == userId1 && r.ToId == userId2 ||
                        r.FromId == userId2 && r.ToId == userId1)
            .ToListAsync();

        switch (requests.Count)
        {
            case 0:
                return FriendshipState.None;
            case 2:
                return FriendshipState.Mutual;
            case 1:
                return requests[0].FromId == userId1
                    ? FriendshipState.RequestPending
                    : FriendshipState.IncomingRequest;
            default:
                throw new ApplicationException("Received more than 2 friendship request records for two users");
        }
    }

    private Task<FriendRequest?> GetFriendshipRecord(string fromId, string toId)
    {
        return DbContext.FriendRequests
            .Where(fr => fr.FromId == fromId && fr.ToId == toId)
            .FirstOrDefaultAsync()!;
    }
}