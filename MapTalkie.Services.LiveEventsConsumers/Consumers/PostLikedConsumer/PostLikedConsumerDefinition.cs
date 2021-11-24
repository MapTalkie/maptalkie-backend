using MapTalkie.Common.Messages.Posts;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Definition;

namespace MapTalkie.Services.LiveEventsConsumers.Consumers.PostLikedConsumer
{
    public class PostLikedConsumerDefinition : ConsumerDefinition<PostLikedConsumer>
    {
        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<PostLikedConsumer> consumerConfigurator)
        {
            consumerConfigurator.Options<BatchOptions>()
                .GroupBy<IPostEngagement, long>(@event => @event.Message.PostId)
                .SetMessageLimit(10000)
                .SetTimeLimit(2000)
                .SetConcurrencyLimit(10);
        }
    }
}