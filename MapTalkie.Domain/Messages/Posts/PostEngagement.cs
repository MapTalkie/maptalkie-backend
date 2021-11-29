namespace MapTalkie.Domain.Messages.Posts
{
    public record PostEngagement : PostMessage
    {
        public PostEngagementType Type { get; set; }
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