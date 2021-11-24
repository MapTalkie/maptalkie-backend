using System;
using MapTalkie.DB.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace MapTalkie.Tests.Unit
{
    public class DbTestsBase : TestsBase, IDisposable
    {
        private readonly string _databaseId = "maptalkie_testing_" + Guid.NewGuid().ToString("n");

        public DbTestsBase()
        {
            var env = new Mock<IWebHostEnvironment>();

            env
                .Setup(e => e.EnvironmentName)
                .Returns(Environments.Development);

            ServiceCollection.AddSingleton(env.Object);

            ServiceCollection.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(
                    $"Host=localhost;Database=maptalkie_testing_{_databaseId};Username=admin;Password=admin",
                    o => o.UseNetTopologySuite());
            });

            var context = ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();
        }

        public AppDbContext Context => ServiceProvider.GetService<AppDbContext>();

        public void Dispose()
        {
            var context = ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureDeleted();
        }
    }
}