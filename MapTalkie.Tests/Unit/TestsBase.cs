using System;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MapTalkie.Tests.Unit;

public class TestsBase
{
    protected readonly IServiceCollection ServiceCollection;

    private IServiceProvider? _serviceProvider;

    public TestsBase()
    {
        ServiceCollection = new ServiceCollection();
        // временный (может быть) хак
        ServiceCollection.AddSingleton(
            typeof(IPublishEndpoint), new Mock<IPublishEndpoint>().Object);
        ServiceCollection.AddLogging();
        ServiceCollection.AddMemoryCache();
    }

    protected IServiceProvider ServiceProvider => _serviceProvider ??= ServiceCollection.BuildServiceProvider();
}