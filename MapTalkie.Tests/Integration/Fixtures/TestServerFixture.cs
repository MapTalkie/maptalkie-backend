using System;
using System.Threading.Tasks;
using MapTalkie.DB;
using MapTalkie.DB.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MapTalkie.Tests.Integration.Fixtures;

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
        InitServer().Wait();
    }

    public string TestUsername { get; private set; }
    public string TestPassword { get; } = "Password01$";

    public TestServer Server { get; }
    public string DatabaseName { get; }

    public void Dispose()
    {
        using var serviceScope = Server.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureDeleted();

        Server.Dispose();
    }

    private async Task InitServer()
    {
        TestUsername = "Test_" + Guid.NewGuid().ToString("N");
        await Server.Services.GetRequiredService<UserManager<User>>().CreateAsync(new User
        {
            UserName = TestUsername,
            Email = $"{TestUsername}@mail.org",
            EmailConfirmed = true
        }, TestPassword);
    }
}

[CollectionDefinition(IntegrationTestsFixtures.Server)]
public class TestServerFixtureCollectionDefinition : ICollectionFixture<TestServerFixture>
{
}