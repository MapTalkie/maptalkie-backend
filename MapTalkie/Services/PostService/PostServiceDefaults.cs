namespace MapTalkie.Services.PostService
{
    public static class PostServiceDefaults
    {
        public const int PopularPostsLimit = 100;
        public const string PopularPostsCacheKeyPrefix = "Post.Popular.Area.";
        public const string PostClustersCacheKeyPrefix = "Post.Clusters.Area.";
        public const string PopularPostsUpdatedInAreaEventPrefix = "PopularPosts.Updated.Area.";
    }
}