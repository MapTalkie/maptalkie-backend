using System.Runtime.Serialization;

namespace MapTalkie.DB
{
    public class CommentLike : LikeBase
    {
        public long CommentId { get; set; }
        [IgnoreDataMember] public PostComment Comment { get; set; } = default!;
    }
}