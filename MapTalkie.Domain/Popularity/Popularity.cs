using System;
using System.Linq.Expressions;
using MapTalkie.DB;

namespace MapTalkie.Domain.Popularity
{
    public static class Popularity
    {
        public const double LikesMultiplier = 1;
        public const double CommentsMultiplier = 3;
        public const double SharesMultiplier = 6;
        public const double DecayCoefficient = .25;
        public const double MinDecay = .0001;
        public const double DefaultRawRank = 1.0;

        public static readonly Expression<Func<Post, double>> PopularityRankProjection =
            p => p.RankDecayFactor * (p.CachedCommentsCount * CommentsMultiplier +
                                      p.CachedLikesCount * LikesMultiplier +
                                      p.CachedSharesCount * SharesMultiplier + DefaultRawRank);
    }
}