namespace MapTalkie.Services.Posts.Messages
{
    public record PostView(
        long Id,
        string UserId,
        string UserName,
        PostPopularity? Popularity = null,
        string? Text = null);
}