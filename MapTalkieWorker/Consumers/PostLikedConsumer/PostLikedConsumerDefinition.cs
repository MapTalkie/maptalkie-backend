using MapTalkieCommon.Messages;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Definition;

namespace MapTalkieWorker.Consumers.PostLikedConsumer
{
    public class PostLikedConsumerDefinition : ConsumerDefinition<PostLikedConsumer>
    {
        public PostLikedConsumerDefinition()
        {
            Endpoint(x => x.PrefetchCount = 10000);
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<PostLikedConsumer> consumerConfigurator)
        {
            consumerConfigurator.Options<BatchOptions>()
                .GroupBy<IPostEngagement, string>(@event => @event.Message.PostId)
                .SetMessageLimit(10000)
                .SetTimeLimit(2000)
                .SetConcurrencyLimit(10);
        }
    }
}