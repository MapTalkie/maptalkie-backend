using System.Linq;
using MapTalkie.DB;

namespace MapTalkie.Services.PopularityProvider
{
    public interface IPopularityProvider
    {
        IQueryable<PopularityRecord<Post>> QueryPopularity(IQueryable<Post> queryable);
        IQueryable<PopularityRecord<Post>> QueryAndOrderByPopularity(IQueryable<Post> queryable);
    }
}