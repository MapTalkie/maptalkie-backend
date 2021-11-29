namespace MapTalkie.Services.Posts.Messages
{
    public record QueryPostsRequest(
        PostOrdering Ordering,
        bool IncludePopularity,
        bool IncludeText,
        bool OnlyAvailable = true
    );
}