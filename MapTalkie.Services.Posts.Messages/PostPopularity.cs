namespace MapTalkie.Services.Posts.Messages
{
    public record PostPopularity(
        double Rank,
        int Shares,
        int Likes,
        int Comments);
}