namespace MapTalkie.Models
{
    public class PostLike : LikeBase
    {
        public long PostId { get; set; }
        public MapPost Post { get; set; } = default!;
    }
}