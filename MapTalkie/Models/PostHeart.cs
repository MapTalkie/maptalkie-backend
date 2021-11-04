namespace MapTalkie.Models
{
    public class PostHeart : HeartBase
    {
        public long PostId { get; set; }
        public MapPost Post { get; set; } = default!;
    }
}