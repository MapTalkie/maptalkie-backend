namespace MapTalkie.Common.Messages.Posts
{
    public interface IPostEngagement
    {
        long PostId { get; }
        string UserId { get; }
        PostEngagementType Type { get; }
    }

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