namespace MapTalkie.Services.PostService
{
    public class PostPopularity
    {
        public long PostId { get; set; }
        public double Rank { get; set; }
        public double FreshRank { get; set; }
        public double TimeDecayFactor { get; set; }
        public int Likes { get; set; }
        public int Comments { get; set; }
        public int Shares { get; set; }
    }
}