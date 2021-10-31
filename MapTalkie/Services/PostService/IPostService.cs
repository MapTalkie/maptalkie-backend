using System;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Models;

namespace MapTalkie.Services.PostService
{
    public interface IPostService
    {
        Task<MapPost> CreateTextPost(
            string text,
            User user,
            Location location,
            bool isOriginalLocation);

        Task<MapPost?> GetPostOrNull(long id);

        IQueryable<MapPost> QueryPostsInArea(
            LatLngBounds bounds,
            DateTime? before = null,
            User? availableFor = null);
    }
}