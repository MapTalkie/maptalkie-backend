using System;
using MapTalkie.Models.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace MaptalkieTests
{
    public class DbTestsBase : TestsBase
    {
        private readonly string _dbId = $"DB-{Guid.NewGuid()}";

        public DbTestsBase()
        {
            var env = new Mock<IWebHostEnvironment>();

            env
                .Setup(e => e.EnvironmentName)
                .Returns(Environments.Development);

            ServiceCollection.AddSingleton(env.Object);


            ServiceCollection.AddDbContext<AppDbContext>(options => { options.UseInMemoryDatabase(_dbId); });
        }

        public AppDbContext Context => ServiceProvider.GetService<AppDbContext>();
    }
}