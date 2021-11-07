using System;
using MapTalkie.Models.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace MaptalkieTests
{
    public class DbTestsBase : TestsBase, IDisposable
    {
        private readonly SqliteConnection _sqliteConnection = new("DataSource=:memory:");

        public DbTestsBase()
        {
            var env = new Mock<IWebHostEnvironment>();

            env
                .Setup(e => e.EnvironmentName)
                .Returns(Environments.Development);

            ServiceCollection.AddSingleton(env.Object);
            ServiceCollection.AddLogging();

            ServiceCollection.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_sqliteConnection, sqlite => sqlite.UseNetTopologySuite());
            });

            var context = ServiceProvider.GetRequiredService<AppDbContext>();
            _sqliteConnection.Open();
            context.Database.EnsureCreated();
        }

        public AppDbContext Context => ServiceProvider.GetService<AppDbContext>();

        public void Dispose()
        {
            _sqliteConnection.Dispose();
        }
    }
}