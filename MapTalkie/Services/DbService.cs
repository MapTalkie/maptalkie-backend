using MapTalkie.Models.Context;

namespace MapTalkie.Services
{
    public class DbService
    {
        protected readonly AppDbContext DbContext;

        public DbService(AppDbContext context)
        {
            DbContext = context;
        }
    }
}