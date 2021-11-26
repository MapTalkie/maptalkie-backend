using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Common.Messages.Posts;
using MapTalkie.Common.Utils;
using MassTransit;
using NetTopologySuite.Geometries;

namespace MapTalkie.Services.LiveEventsConsumers.Consumers.PostCreatedConsumer
{
    public class PostCreatedConsumer : IConsumer<Batch<IPostCreated>>
    {
        public async Task Consume(ConsumeContext<Batch<IPostCreated>> context)
        {
            var updates = new Dictionary<AreaId, GeoAreaUpdate>();

            foreach (var ctx in context.Message)
            {
                var area = AreaId.FromPoint((Point)ctx.Message.Location);
                if (updates.ContainsKey(area)) updates[area].NewPosts.Add(ctx.Message);
            }

            await context.Publish<IGeoUpdate>(new
            {
                Updates = updates.Values.ToList()
            });
        }
    }
}