using System.Runtime.Serialization;

namespace MapTalkie.DB;

public class PostLike : LikeBase
{
    public long PostId { get; set; }
    [IgnoreDataMember] public Post Post { get; set; } = default!;
}