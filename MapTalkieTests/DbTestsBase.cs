using System;
using MapTalkie.Models.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MaptalkieTests
{
    public class DbTestsBase : TestsBase
    {
        private readonly string _dbId = $"DB-{Guid.NewGuid()}";

        public DbTestsBase()
        {
            ServiceCollection.AddDbContext<AppDbContext>(
                options => options.UseInMemoryDatabase(_dbId));
        }

        public AppDbContext Context => ServiceProvider.GetService<AppDbContext>();
    }
}