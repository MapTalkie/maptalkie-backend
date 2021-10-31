using System;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Models;

namespace MapTalkie.Services.CommentService
{
    public interface ICommentService
    {
        Task<PostComment> CreateComment(long postId, int senderId, string text);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commentId"></param>
        /// <param name="senderId"></param>
        /// <param name="text"></param>
        /// <exception cref="CommentNotFoundException">Если родительский комментарий не был найден</exception>
        /// <returns>Задача с созданным комментарием</returns>
        Task<PostComment> ReplyToComment(long commentId, int senderId, string text);

        Task<PostComment> UpdateComment(long commentId, Action<PostComment> updateFunction);

        Task<PostComment?> GetCommentOrNull(long commentId);

        Task<CommentReaction> ReactTo(long commentId, int userId, ReactionType? reactionType = null);

        Task RemoveReaction(long commentId, int userId);

        IQueryable<PostComment> QueryComments(long postId);
    }
}