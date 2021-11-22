using System.Linq;
using System.Threading.Tasks;
using MapTalkieDB;
using NetTopologySuite.Geometries;

namespace MapTalkie.Services.PostService
{
    public interface IPostService
    {
        Task<Post> CreateTextPost(
            string text,
            string userId,
            Point location,
            bool isOriginalLocation);

        Task<Post?> GetPostOrNull(string id, bool includeUnavailable = false);

        IQueryable<Post> QueryPosts(
            Geometry? geometry = null,
            bool? available = true,
            User? availableFor = null);

        IQueryable<Post> QueryPopularPosts(
            Geometry? geometry = null,
            bool? available = true,
            User? availableFor = null);

        IQueryable<PostPopularity> QueryPopularity(
            Geometry? geometry = null,
            bool? available = true,
            User? availableFor = null);

        IQueryable<Post> QueryByComments(
            Geometry? geometry = null,
            User? availableFor = null);

        IQueryable<Post> QueryByLikes(
            Geometry? geometry = null,
            User? availableFor = null);

        IQueryable<Post> QueryByShares(
            Geometry? geometry = null,
            User? availableFor = null);

        Task<bool> IsAvailable(string id);

        Task<PostPopularity> GetPopularity(string id);

        Task FavoritePost(Post post, string userId);

        Task UnFavoritePost(Post post, string userId);

        Task DeletePost(Post post);
    }
}