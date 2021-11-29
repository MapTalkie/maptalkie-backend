namespace MapTalkie.Services.Posts.Messages
{
    public record CreatePostRequest(
        string UserId,
        string Text,
        double Latitude,
        double Longitude,
        bool IsGenuineLocation);
}