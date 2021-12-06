using System;
using System.Linq.Expressions;
using MapTalkie.DB;

namespace MapTalkie.Views
{
    public record CommentView(
        string UserName,
        string UserId,
        DateTime CreatedAt,
        long Id,
        long PostId,
        string Text,
        int Likes)
    {
        public static Expression<Func<PostComment, CommentView>> Projection = c => new CommentView(
            c.Sender.UserName, c.SenderId, c.CreatedAt, c.Id, c.PostId, c.Text, c.CachedLikesCount);
    }
}