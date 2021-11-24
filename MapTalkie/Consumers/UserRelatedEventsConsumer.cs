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
        IConsumer<IPrivateMessage>
    {
        private readonly IHubContext<MainUserHub> _hubContext;

        public UserRelatedEventsConsumer(IHubContext<MainUserHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task Consume(ConsumeContext<IAccumulatedEngagementEvent> context)
        {
            return _hubContext.Clients.Group(MapTalkieGroups.PostUpdatesPrefix + context.Message.PostId)
                .SendAsync("Engagement", new
                {
                    Id = context.Message.PostId,
                    Comments = context.Message.Comments,
                    Likes = context.Message.Likes,
                    Shares = context.Message.Shares,
                    Latitude = context.Message.LocationDescriptor.Latitude,
                    Longitude = context.Message.LocationDescriptor.Longitude
                });
        }

        public async Task Consume(ConsumeContext<IGeoUpdate> context)
        {
            foreach (var update in context.Message.Updates)
            {
                await _hubContext.Clients
                    .Group(MapTalkieGroups.AreaUpdatesPrefix + MainUserHub.SubscriptionType.Latest + update.Id)
                    .SendAsync("NewPosts", new
                    {
                        Posts = update.NewPosts.Select(p => new
                        {
                            Id = p.PostId,
                            UserId = p.UserId,
                            Latitude = p.Location.Latitude,
                            Longitude = p.Location.Longitude
                        })
                    });
            }
        }

        public async Task Consume(ConsumeContext<IPrivateMessage> context)
        {
            await _hubContext.Clients.Group(MapTalkieGroups.ConversationPrefix + context.Message.ConversationId)
                .SendAsync("PrivateMessage", new
                {
                    Id = context.Message.MessageId,
                    context.Message.Text,
                    context.Message.RecipientId,
                    context.Message.SenderId,
                    context.Message.ConversationId,
                });
        }
    }
}