using System;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MaptalkieTests
{
    public class TestsBase
    {
        protected readonly IServiceCollection ServiceCollection;

        public TestsBase()
        {
            ServiceCollection = new ServiceCollection();
            // временный (может быть) хак
            ServiceCollection.AddSingleton(
                typeof(IPublishEndpoint), new Mock<IPublishEndpoint>().Object);
            ServiceCollection.AddLogging();
            ServiceCollection.AddMemoryCache();
            ServiceCollection.AddSingleton<IEventBus, LocalEventBus>();
        }

        protected IServiceProvider ServiceProvider => ServiceCollection.BuildServiceProvider();
    }
}