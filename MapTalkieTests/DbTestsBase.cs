using MapTalkie.Models.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MaptalkieTests
{
    public class DbTestsBase : TestsBase
    {
        public DbTestsBase()
        {
            ServiceCollection.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("testing"));
        }

        public AppDbContext Context => ServiceProvider.GetService<AppDbContext>();
    }
}