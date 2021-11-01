namespace MapTalkie.Models
{
    public enum ReactionType : byte
    {
        HEART = 1,
        SMILE = 2,
        ANGRY = 3,
        FUNNY = 4,
    }

    public class ReactionBase
    {
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = default!;
        public ReactionType Type { get; set; } = ReactionType.HEART;
    }
}