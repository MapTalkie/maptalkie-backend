using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Domain.Messages.Posts;
using MapTalkie.Domain.Messages.PrivateMessages;
using MapTalkie.Domain.Utils;
using MapTalkie.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace MapTalkie.Consumers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class UserRelatedEventsConsumer :
        IConsumer<EngagementUpdate>,
        IConsumer<GeoUpdates>,
        IConsumer<PrivateMessageBase>
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
                    .Group(MapTalkieGroups.AreaUpdatesPrefix + UserHub.SubscriptionType.Latest + update.Id)
                    .SendAsync(UserHub.PostsUpdate, new
                    {
                        Posts = update.NewPosts.Select(p => new
                        {
                            Id = p.PostId,
                            p.UserId,
                            Location = MapConvert.ToLatLon(p.Location)
                        }).ToList(),
                        Type = UserHub.SubscriptionType.Latest
                    });
        }

        public async Task Consume(ConsumeContext<PrivateMessageBase> context)
        {
            var groupProxy =
                _hubContext.Clients.Group(MapTalkieGroups.Conversation(context.Message.SenderId,
                    context.Message.RecipientId));
            switch (context.Message)
            {
                case PrivateMessage privateMessage:
                    await groupProxy.SendAsync(UserHub.DirectMessage, new
                    {
                        Id = privateMessage.MessageId,
                        privateMessage.Text,
                        privateMessage.RecipientId,
                        privateMessage.SenderId
                    });
                    break;
                case PrivateMessageDeleted privateMessageDeleted:
                    await groupProxy.SendAsync(UserHub.DirectMessageDeleted, new
                    {
                        privateMessageDeleted.MessageId,
                        privateMessageDeleted.RecipientId
                    });
                    break;
            }
        }
    }
}