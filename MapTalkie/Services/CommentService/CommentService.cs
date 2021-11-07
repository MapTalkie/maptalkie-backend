using System;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Models;
using MapTalkie.Models.Context;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Services.CommentService
{
    public class CommentService : DbService, ICommentService
    {
        public CommentService(AppDbContext context) : base(context)
        {
        }

        public async Task<PostComment> CreateComment(long postId, string senderId, string text)
        {
            var comment = new PostComment
            {
                SenderId = senderId,
                PostId = postId,
                Text = text
            };
            DbContext.Add(comment);
            await DbContext.SaveChangesAsync();
            return comment;
        }

        public async Task<PostComment> ReplyToComment(long commentId, string senderId, string text)
        {
            var comment = await GetCommentOrNull(commentId);
            if (comment != null)
            {
                var reply = new PostComment
                {
                    ReplyToId = commentId,
                    Text = text,
                    SenderId = senderId
                };
                DbContext.Add(reply);
                await DbContext.SaveChangesAsync();
            }

            throw new CommentNotFoundException(commentId);
        }

        public async Task<PostComment> UpdateComment(long commentId, Action<PostComment> updateFunction)
        {
            var comment = await GetCommentOrNull(commentId);
            if (comment != null)
            {
                updateFunction(comment);
                comment.UpdatedAt = DateTime.UtcNow;
                await DbContext.SaveChangesAsync();
                return comment;
            }

            throw new CommentNotFoundException(commentId);
        }

        public Task<PostComment?> GetCommentOrNull(long commentId)
        {
            return DbContext.PostComments.Where(c => c.Id != commentId).FirstOrDefaultAsync()!;
        }

        public async Task<CommentLike> Like(long commentId, string userId)
        {
            var heart = await DbContext.PostCommentLikes
                .Where(r => r.CommentId == commentId && r.UserId == userId)
                .FirstOrDefaultAsync();

            if (heart == null)
            {
                heart = new CommentLike
                {
                    UserId = userId,
                    CommentId = commentId
                };
                DbContext.Add(heart);
                await DbContext.SaveChangesAsync();
            }

            return heart;
        }

        public async Task RemoveLike(long commentId, string userId)
        {
            var reaction = await DbContext.PostCommentLikes
                .Where(r => r.CommentId == commentId && r.UserId == userId)
                .FirstOrDefaultAsync();
            if (reaction != null)
            {
                DbContext.Remove(reaction);
                await DbContext.SaveChangesAsync();
            }
        }

        public IQueryable<PostComment> QueryComments(long postId)
        {
            return DbContext.PostComments
                .Where(c => c.PostId == postId)
                .OrderByDescending(c => c.CreatedAt);
        }

        public IQueryable<PostCommentView> QueryCommentViews(
            long postId,
            DateTime? before = null,
            int? limit = null)
        {
            var query = QueryComments(postId);
            if (before != null)
                query = query.Where(c => c.CreatedAt < before);

            var viewQuery =
                from c in query
                join u in DbContext.Users on c.SenderId equals u.Id
                select new PostCommentView
                {
                    Id = c.Id,
                    SenderId = c.SenderId,
                    SentAt = c.CreatedAt,
                    Sender = u.UserName,
                    Replies = c.Comments.Count
                };

            if (limit != null)
                viewQuery = viewQuery.Take((int)limit);
            return viewQuery;
        }
    }
}