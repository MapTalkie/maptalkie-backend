namespace MapTalkie.Services.PopularityProvider
{
    public record PopularityRecord<T>(
        double Rank,
        int Likes,
        int Shares,
        int Comments,
        T View
    );
}