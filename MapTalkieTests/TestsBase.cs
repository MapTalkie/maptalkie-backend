using System;
using Microsoft.Extensions.DependencyInjection;

namespace MaptalkieTests
{
    public class TestsBase
    {
        protected readonly IServiceCollection ServiceCollection;

        public TestsBase()
        {
            ServiceCollection = new ServiceCollection();
        }

        protected IServiceProvider ServiceProvider => ServiceCollection.BuildServiceProvider();
    }
}