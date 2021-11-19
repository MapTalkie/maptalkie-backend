using MapTalkie.Utils.Cache;

namespace MapTalkie.Services.PostService
{
    public static class PostServiceDefaults
    {
        public const int PopularPostsLimit = 100;
        public static CacheKey PostsKey = new("Posts");
        public static CacheKey PopularPostsKey = PostsKey["Popular"];
        public static CacheKey PopularPostsInAreaKey = PopularPostsKey["Area"];
    }
}