using System.Runtime.Serialization;

namespace MapTalkie.Services.Posts.Models
{
    public class CommentLike
    {
        public string UserId { get; set; } = default!;
        public long CommentId { get; set; }
        [IgnoreDataMember] public PostComment Comment { get; set; } = default!;
    }
}