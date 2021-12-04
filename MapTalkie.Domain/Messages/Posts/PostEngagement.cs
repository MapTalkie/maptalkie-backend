using NetTopologySuite.Geometries;

namespace MapTalkie.Domain.Messages.Posts
{
    public record PostEngagement(
        long PostId,
        string UserId,
        Point Location,
        PostEngagementType Type) : PostMessage(PostId, UserId, Location);

    public enum PostEngagementType
    {
        Favorite,
        FavoriteRemoved,
        Share,
        ShareRemoved,
        Comment,
        CommentRemoved
    }
}