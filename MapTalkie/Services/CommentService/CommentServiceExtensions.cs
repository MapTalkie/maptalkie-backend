using System.Collections.Generic;
using System.Threading.Tasks;
using MapTalkieDB;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Services.CommentService
{
    public static class CommentServiceExtensions
    {
        public static Task<List<PostComment>> GetPostComments(this ICommentService service, string postId)
        {
            return service.QueryComments(postId).ToListAsync();
        }
    }
}