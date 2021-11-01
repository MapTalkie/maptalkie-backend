using System;
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

        IQueryable<MapPost> QueryPostsInArea(
            Geometry geometry,
            DateTime? before = null,
            User? availableFor = null);

        Task<bool> IsAvailable(long id);
    }
}