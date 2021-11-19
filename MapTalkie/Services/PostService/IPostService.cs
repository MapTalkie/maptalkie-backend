using System;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Models;
using MapTalkie.Services.PostService.Events;
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

        Task<Post?> GetPostOrNull(long id, bool includeUnavailable = false);

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

        Task<bool> IsAvailable(long id);

        Task<PostPopularity> GetPopularity(long postId);

        Task FavoritePost(Post post, string userId);

        Task UnFavoritePost(Post post, string userId);

        IDisposable SubscribeToEngagement(long postId, Func<PostEngagement, Task> callback);

        IDisposable SubscribeToEngagement(Polygon polygon, Func<PostEngagement, Task> callback);
    }
}