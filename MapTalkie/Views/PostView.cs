using System;
using MapTalkie.DB;

namespace MapTalkie.Views
{
    public record PostView(
        long Id,
        string UserId,
        string UserName,
        DateTime CreatedAt,
        int Likes,
        int Comments,
        int Shares,
        string Text,
        bool IsOriginalLocation)
    {
        public PostView(Post post) :
            this(post.Id, post.UserId, post.User.UserName, post.CreatedAt,
                post.CachedLikesCount, post.CachedSharesCount, post.CachedCommentsCount, post.Text,
                post.IsOriginalLocation)
        {
        }
    }
}