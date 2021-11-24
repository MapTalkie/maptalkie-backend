using System.Collections.Generic;
using System.Threading.Tasks;
using MapTalkie.DB;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Services.CommentService
{
    public static class CommentServiceExtensions
    {
        public static Task<List<PostComment>> GetPostComments(this ICommentService service, long postId)
        {
            return service.QueryComments(postId).ToListAsync();
        }
    }
}