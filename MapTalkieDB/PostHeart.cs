namespace MapTalkieDB
{
    public class PostLike : LikeBase
    {
        public string PostId { get; set; }
        public Post Post { get; set; } = default!;
    }
}