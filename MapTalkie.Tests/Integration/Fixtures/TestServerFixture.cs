using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace MapTalkie.Tests.Integration.Fixtures
{
    public class TestServerFixture : IDisposable
    {
        public TestServer Server { get; private set; }

        public void Dispose()
        {
            var webHost = new WebHostBuilder()
                .UseStartup<Startup>()
                .UseEnvironment("Testing");

            Server = new TestServer(webHost);
        }
    }
}