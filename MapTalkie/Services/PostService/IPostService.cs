using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Models;
using NetTopologySuite.Geometries;

namespace MapTalkie.Services.PostService
{
    public interface IPostService
    {
        Task<MapPost> CreateTextPost(
            string text,
            string userId,
            Point location,
            bool isOriginalLocation);

        Task<MapPost?> GetPostOrNull(long id, bool includeUnavailable = false);

        IQueryable<MapPost> QueryPosts(
            Geometry? geometry = null,
            bool? available = true,
            User? availableFor = null);

        IQueryable<MapPost> QueryPopularPosts(
            int limit = PostServiceDefaults.PopularPostsLimit,
            Geometry? geometry = null,
            bool? available = true,
            User? availableFor = null);

        Task<bool> IsAvailable(long id);

        Task<double> GetPopularity(long postId);

        Task FavoritePost(MapPost post, string userId);

        Task UnfavoritePost(MapPost post, string userId);

        Task<MapLayerState> GetLayerState(
            Polygon area,
            User? availableFor = null,
            int limit = PostServiceDefaults.PopularPostsLimit);
    }
}