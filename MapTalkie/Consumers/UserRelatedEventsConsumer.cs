using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Domain.Messages.Posts;
using MapTalkie.Domain.Messages.PrivateMessages;
using MapTalkie.Domain.Utils;
using MapTalkie.Hubs;
using MapTalkie.Views;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace MapTalkie.Consumers;

// ReSharper disable once ClassNeverInstantiated.Global
public class UserRelatedEventsConsumer :
    IConsumer<EngagementUpdate>,
    IConsumer<GeoUpdates>,
    IConsumer<PrivateMessage>,
    IConsumer<PrivateMessageDeleted>
{
    private readonly IHubContext<UserHub> _hubContext;

    public UserRelatedEventsConsumer(IHubContext<UserHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task Consume(ConsumeContext<EngagementUpdate> context)
    {
        return _hubContext.Clients.Group(MapTalkieGroups.PostUpdatesPrefix + context.Message.PostId)
            .SendAsync(UserHub.PostEngagement, new
            {
                Id = context.Message.PostId.ToString(),
                context.Message.Comments,
                context.Message.Likes,
                context.Message.Shares,
                context.Message.Location
            });
    }

    public async Task Consume(ConsumeContext<GeoUpdates> context)
    {
        foreach (var update in context.Message.Updates)
            await _hubContext.Clients
                .Group(MapTalkieGroups.AreaUpdatesPrefix + update.Id)
                .SendAsync(UserHub.PostsUpdate, new
                {
                    Posts = update.NewPosts.Select(p => new
                    {
                        Id = p.PostId,
                        p.UserId,
                        Location = MapConvert.ToLatLon(p.Location),
                        Preview = p.PostTextPreview
                    }).ToList()
                });
    }

    public async Task Consume(ConsumeContext<PrivateMessage> context)
    {
        await _hubContext.Clients.Groups(
            MapTalkieGroups.Messages + context.Message.RecipientId,
            MapTalkieGroups.Messages + context.Message.SenderId).SendAsync(UserHub.DirectMessage, new
        {
            Id = context.Message.MessageId,
            context.Message.Text,
            Recipient = new UserInMessageView
            {
                Id = context.Message.RecipientId,
                UserName = context.Message.RecipientUsername
            },
            Sender = new UserInMessageView
            {
                Id = context.Message.SenderId,
                UserName = context.Message.SenderUsername
            }
        });
    }

    public async Task Consume(ConsumeContext<PrivateMessageDeleted> context)
    {
        await _hubContext.Clients.Group(MapTalkieGroups.ConversationPrefix + context.Message.SenderId).SendAsync(
            UserHub.DirectMessageDeleted, new
            {
                context.Message.MessageId,
                context.Message.RecipientId
            });
    }
}