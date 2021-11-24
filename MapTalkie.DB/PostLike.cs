namespace MapTalkie.DB
{
    public class PostLike : LikeBase
    {
        public long PostId { get; set; }
        public Post Post { get; set; } = default!;
    }
}