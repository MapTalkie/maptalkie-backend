using System.Collections.Generic;
using System.Threading.Tasks;
using MapTalkie.Models;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace MapTalkie.Services.PostService
{
    public static class PostServiceExtensions
    {
        public static Task<List<MapPost>> GetPosts(
            this IPostService service,
            Geometry? geometry = null,
            bool? available = true,
            User? availableFor = null) => service.QueryPosts(geometry, available, availableFor).ToListAsync();

        public static Task<List<MapPost>> GetPopularPosts(
            this IPostService service,
            int limit = PostServiceDefaults.PopularPostsLimit,
            Geometry? geometry = null,
            bool? available = true,
            User? availableFor = null) =>
            service.QueryPopularPosts(limit, geometry, available, availableFor).ToListAsync();
    }
}