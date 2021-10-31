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

        public async Task<PostComment> CreateComment(long postId, int senderId, string text)
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

        public async Task<PostComment> ReplyToComment(long commentId, int senderId, string text)
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

        public async Task<CommentReaction> ReactTo(long commentId, int userId, ReactionType? reactionType = null)
        {
            var reaction = await DbContext.PostCommentReactions
                .Where(r => r.CommentId == commentId && r.UserId == userId)
                .FirstOrDefaultAsync();

            if (reaction == null)
            {
                reaction = new CommentReaction
                {
                    UserId = userId,
                    CommentId = commentId,
                    Type = reactionType ?? ReactionType.HEART
                };
                DbContext.Add(reaction);
                await DbContext.SaveChangesAsync();
            }
            else if (reaction.Type != (reactionType ?? ReactionType.HEART))
            {
                reaction.Type = reactionType ?? ReactionType.HEART;
                await DbContext.SaveChangesAsync();
            }

            return reaction;
        }

        public async Task RemoveReaction(long commentId, int userId)
        {
            var reaction = await DbContext.PostCommentReactions
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
            return DbContext.PostComments.Where(c => c.PostId == postId);
        }
    }
}