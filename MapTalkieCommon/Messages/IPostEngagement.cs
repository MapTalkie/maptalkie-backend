namespace MapTalkieCommon.Messages
{
    public enum PostEngagementType
    {
        Favorite,
        FavoriteRemoved,
        Share,
        ShareRemoved,
        Comment,
        CommentRemoved
    }

    public interface IPostEngagement
    {
        string PostId { get; }
        string UserId { get; }
        PostEngagementType Type { get; }
    }
}