using System.Linq;
using MapTalkie.DB;
using MapTalkie.Domain.Popularity;

namespace MapTalkie.Services.PopularityProvider
{
    public class PopularityProvider : IPopularityProvider
    {
        public IQueryable<PopularityRecord<Post>> QueryPopularity(IQueryable<Post> queryable)
        {
            return from e in queryable
                let comments = e.CachedCommentsCount + e.Comments.Count(c => c.CreatedAt > e.CacheUpdatedAt)
                let shares = e.CachedCommentsCount + e.Shares.Count(c => c.CreatedAt > e.CacheUpdatedAt)
                let likes = e.CachedLikesCount + e.Likes.Count(c => c.CreatedAt > e.CacheUpdatedAt)
                let rank =
                    likes * PopularityConstants.LikesMultiplier +
                    comments * PopularityConstants.CommentsMultiplier +
                    shares * PopularityConstants.SharesMultiplier
                select new PopularityRecord<Post>(rank, likes, shares, comments, e);
        }

        public IQueryable<PopularityRecord<Post>> QueryAndOrderByPopularity(IQueryable<Post> queryable)
        {
            return from e in queryable
                let comments = e.CachedCommentsCount + e.Comments.Count(c => c.CreatedAt > e.CacheUpdatedAt)
                let shares = e.CachedCommentsCount + e.Shares.Count(c => c.CreatedAt > e.CacheUpdatedAt)
                let likes = e.CachedLikesCount + e.Likes.Count(c => c.CreatedAt > e.CacheUpdatedAt)
                let rank =
                    likes * PopularityConstants.LikesMultiplier +
                    comments * PopularityConstants.CommentsMultiplier +
                    shares * PopularityConstants.SharesMultiplier
                orderby rank
                select new PopularityRecord<Post>(rank, likes, shares, comments, e);
        }
    }
}