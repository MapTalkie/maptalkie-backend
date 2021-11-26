using System;
using MapTalkie.DB.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MapTalkie.Tests.Integration.Fixtures
{
    public class TestServerFixture : IDisposable
    {
        public TestServerFixture()
        {
            DatabaseName = $"maptalkie_testing_integration_{Guid.NewGuid():N}";
            var webHost = new WebHostBuilder()
                .ConfigureAppConfiguration(cfg => { cfg.AddJsonFile("appsettings.json").AddEnvironmentVariables(); })
                .UseStartup<TestStartup>()
                .UseEnvironment("Testing");

            Server = new TestServer(webHost);
        }

        public TestServer Server { get; private set; }
        public string DatabaseName { get; private set; }

        public void Dispose()
        {
            using var serviceScope = Server.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureDeleted();

            Server.Dispose();
        }
    }

    [CollectionDefinition(IntegrationTestsFixtures.Server)]
    public class TestServerFixtureCollectionDefinition : ICollectionFixture<TestServerFixture>
    {
    }
}