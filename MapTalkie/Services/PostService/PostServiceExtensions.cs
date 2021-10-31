using System.Collections.Generic;
using System.Threading.Tasks;
using MapTalkie.Models;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Services.PostService
{
    public static class PostServiceExtensions
    {
        public static Task<List<MapPost>> GetMapPostsInArea(this IPostService service, LatLngBounds bounds)
            => service.QueryPostsInArea(bounds).ToListAsync();
    }
}