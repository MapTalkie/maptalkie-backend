using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Common.Messages;
using MapTalkie.Common.Messages.Posts;
using MapTalkie.Common.Messages.PrivateMessages;
using MapTalkie.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace MapTalkie.Consumers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class UserRelatedEventsConsumer :
        IConsumer<IAccumulatedEngagementEvent>,
        IConsumer<IGeoUpdate>,
        IConsumer<IPrivateMessageBase>
    {
        private readonly IHubContext<UserHub> _hubContext;

        public UserRelatedEventsConsumer(IHubContext<UserHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task Consume(ConsumeContext<IAccumulatedEngagementEvent> context)
        {
            return _hubContext.Clients.Group(MapTalkieGroups.PostUpdatesPrefix + context.Message.PostId)
                .SendAsync(UserHub.PostEngagement, new
                {
                    Id = context.Message.PostId,
                    context.Message.Comments,
                    context.Message.Likes,
                    context.Message.Shares,
                    context.Message.LocationDescriptor.Latitude,
                    context.Message.LocationDescriptor.Longitude
                });
        }

        public async Task Consume(ConsumeContext<IGeoUpdate> context)
        {
            foreach (var update in context.Message.Updates)
                await _hubContext.Clients
                    .Group(MapTalkieGroups.AreaUpdatesPrefix + UserHub.SubscriptionType.Latest + update.Id)
                    .SendAsync("NewPosts", new
                    {
                        Posts = update.NewPosts.Select(p => new
                        {
                            Id = p.PostId,
                            p.UserId,
                            p.Location.Latitude,
                            p.Location.Longitude
                        })
                    });
        }

        public async Task Consume(ConsumeContext<IPrivateMessageBase> context)
        {
            var groupProxy =
                _hubContext.Clients.Group(MapTalkieGroups.Conversation(context.Message.SenderId,
                    context.Message.RecipientId));
            switch (context.Message)
            {
                case IPrivateMessage privateMessage:
                    await groupProxy.SendAsync(UserHub.DirectMessage, new
                    {
                        Id = privateMessage.MessageId,
                        privateMessage.Text,
                        privateMessage.RecipientId,
                        privateMessage.SenderId
                    });
                    break;
                case IPrivateMessageDeleted privateMessageDeleted:
                    await groupProxy.SendAsync(UserHub.DirectMessageDeleted, new
                    {
                        privateMessageDeleted.MessageId,
                        privateMessageDeleted.RecipientId
                    });
                    break;
            }
            // TODO событие Update?
        }
    }
}