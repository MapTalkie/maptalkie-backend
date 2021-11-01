using System;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Models;

namespace MapTalkie.Services.CommentService
{
    public interface ICommentService
    {
        Task<PostComment> CreateComment(long postId, string senderId, string text);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commentId"></param>
        /// <param name="senderId"></param>
        /// <param name="text"></param>
        /// <exception cref="CommentNotFoundException">Если родительский комментарий не был найден</exception>
        /// <returns>Задача с созданным комментарием</returns>
        Task<PostComment> ReplyToComment(long commentId, string senderId, string text);

        Task<PostComment> UpdateComment(long commentId, Action<PostComment> updateFunction);

        Task<PostComment?> GetCommentOrNull(long commentId);

        Task<CommentReaction> ReactTo(long commentId, string userId, ReactionType? reactionType = null);

        Task RemoveReaction(long commentId, string userId);

        IQueryable<PostComment> QueryComments(long postId);

        IQueryable<PostCommentView> QueryCommentViews(
            long postId,
            DateTime? before = null,
            int? limit = null);
    }
}