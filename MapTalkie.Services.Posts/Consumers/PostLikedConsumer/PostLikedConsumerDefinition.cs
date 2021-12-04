using MapTalkie.Domain.Messages.Posts;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Definition;

namespace MapTalkie.Services.Posts.Consumers.PostLikedConsumer
{
    public class PostLikedConsumerDefinition : ConsumerDefinition<PostLikedConsumer>
    {
        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<PostLikedConsumer> consumerConfigurator)
        {
            consumerConfigurator.Options<BatchOptions>()
                .GroupBy<PostEngagement, long>(@event => @event.Message.PostId)
                .SetMessageLimit(1000)
                .SetTimeLimit(1000)
                .SetConcurrencyLimit(10);
        }
    }
}