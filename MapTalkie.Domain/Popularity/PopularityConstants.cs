namespace MapTalkie.Domain.Popularity
{
    public static class PopularityConstants
    {
        public const double LikesMultiplier = 1;
        public const double CommentsMultiplier = 3;
        public const double SharesMultiplier = 6;
        public const double DecayCoefficient = .25;
        public const double MinDecay = .0001;
    }
}