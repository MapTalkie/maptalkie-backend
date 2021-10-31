namespace MapTalkie.Models
{
    public class CommentReaction : ReactionBase
    {
        public long CommentId { get; set; }
        public PostComment Comment { get; set; } = default!;
    }
}