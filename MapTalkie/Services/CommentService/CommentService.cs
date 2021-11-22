using System;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.MessagesImpl;
using MapTalkieCommon.Messages;
using MapTalkieDB;
using MapTalkieDB.Context;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Services.CommentService
{
    public class CommentService : DbService, ICommentService
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public CommentService(AppDbContext context, IPublishEndpoint publishEndpoint) : base(context)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task<PostComment> CreateComment(string postId, string senderId, string text)
        {
            var comment = new PostComment
            {
                SenderId = senderId,
                PostId = postId,
                Text = text
            };
            DbContext.Add(comment);
            await DbContext.SaveChangesAsync();
            await _publishEndpoint.Publish<IPostEngagement>(new PostEngagementEvent
            {
                PostId = postId,
                UserId = senderId,
                Type = PostEngagementType.Comment
            });
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
                await _publishEndpoint.Publish<IPostEngagement>(new PostEngagementEvent
                {
                    PostId = comment.PostId,
                    UserId = senderId,
                    Type = PostEngagementType.Comment
                });
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

        public async Task RemoveComment(PostComment comment)
        {
            if (!comment.Available)
                return;
            comment.Available = false;
            await DbContext.SaveChangesAsync();
            await _publishEndpoint.Publish<IPostEngagement>(new PostEngagementEvent
            {
                Type = PostEngagementType.CommentRemoved,
                UserId = comment.SenderId,
                PostId = comment.PostId
            });
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

        public IQueryable<PostComment> QueryComments(string postId)
        {
            return DbContext.PostComments
                .Where(c => c.PostId == postId)
                .OrderByDescending(c => c.CreatedAt);
        }

        public IQueryable<PostCommentView> QueryCommentViews(
            string postId,
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