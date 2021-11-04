namespace MapTalkie.Models
{
    public class CommentHeart : HeartBase
    {
        public long CommentId { get; set; }
        public PostComment Comment { get; set; } = default!;
    }
}