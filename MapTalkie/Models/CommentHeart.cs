namespace MapTalkie.Models
{
    public class CommentLike : LikeBase
    {
        public long CommentId { get; set; }
        public PostComment Comment { get; set; } = default!;
    }
}