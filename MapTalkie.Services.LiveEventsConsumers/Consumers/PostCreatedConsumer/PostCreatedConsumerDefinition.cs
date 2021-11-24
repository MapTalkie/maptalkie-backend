using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Definition;

namespace MapTalkie.Services.LiveEventsConsumers.Consumers.PostCreatedConsumer
{
    public class PostCreatedConsumerDefinition : ConsumerDefinition<PostCreatedConsumer>
    {
        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<PostCreatedConsumer> consumerConfigurator)
        {
            consumerConfigurator.Options<BatchOptions>()
                .SetMessageLimit(1000)
                .SetTimeLimit(500)
                .SetConcurrencyLimit(1);
        }
    }
}