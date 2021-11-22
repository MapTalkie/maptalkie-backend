using System;
using System.Linq;
using System.Threading.Tasks;
using MapTalkieDB;

namespace MapTalkie.Services.CommentService
{
    public interface ICommentService
    {
        Task<PostComment> CreateComment(string postId, string senderId, string text);

        /// <summary>
        /// </summary>
        /// <param name="commentId"></param>
        /// <param name="senderId"></param>
        /// <param name="text"></param>
        /// <exception cref="CommentNotFoundException">Если родительский комментарий не был найден</exception>
        /// <returns>Задача с созданным комментарием</returns>
        Task<PostComment> ReplyToComment(long commentId, string senderId, string text);

        Task<PostComment> UpdateComment(long commentId, Action<PostComment> updateFunction);

        Task RemoveComment(PostComment comment);

        Task<PostComment?> GetCommentOrNull(long commentId);

        Task<CommentLike> Like(long commentId, string userId);

        Task RemoveLike(long commentId, string userId);

        IQueryable<PostComment> QueryComments(string postId);

        IQueryable<PostCommentView> QueryCommentViews(
            string postId,
            DateTime? before = null,
            int? limit = null);
    }
}