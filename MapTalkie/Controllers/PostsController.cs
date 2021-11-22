using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Services.CommentService;
using MapTalkie.Services.PostService;
using MapTalkie.Utils;
using MapTalkieCommon.Utils;
using MapTalkieDB;
using MapTalkieDB.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace MapTalkie.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PostsController : AuthorizedController
    {
        private readonly AppDbContext _context;
        private readonly IPostService _postService;

        public PostsController(
            AppDbContext dbContext,
            IPostService postService,
            UserManager<User> userManager) : base(dbContext)
        {
            _postService = postService;
            _context = dbContext;
        }

        #region Delete post

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost([FromRoute] string id)
        {
            var post = await _postService.GetPostOrNull(id);
            if (post == null)
                return NotFound();
            if (post.UserId != UserId)
                return Unauthorized();
            _context.Remove(post);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        #endregion

        #region Получить пост(ы)

        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost([FromRoute] string id)
        {
            var post = await _postService.GetPostOrNull(id);
            if (post != null)
                return post;
            return NotFound();
        }

        [HttpGet("geo/{polygon}")]
        public async Task<ListResponse<Post>> FindPostsInArea([FromRoute] Polygon polygon)
        {
            return new ListResponse<Post>(await _postService.QueryPosts(polygon).Take(50).ToListAsync());
        }

        [HttpGet("geo/closest/{point}")]
        public async Task<ActionResult> FindPostsNearby([FromRoute] Point point)
        {
            var posts = await _postService
                .QueryPosts(availableFor: await RequireUser())
                .Select(p => new { Distance = p.Location.Distance(point), Post = p })
                .OrderBy(p => p.Distance)
                .Select(p => new { Post = SelectPostView(p.Post), p.Distance })
                .ToListAsync();
            return Json(ListResponse.Of(posts));
        }

        [HttpGet("geo/popular/{polygon}")]
        public async Task<IActionResult> GetPopularPosts(
            [FromRoute] Polygon polygon,
            [FromQuery] int limit = 50)
        {
            var posts = await _postService
                .QueryPopularPosts(polygon, availableFor: await GetUser())
                .Select(p => SelectPostView(p))
                .Take(limit)
                .ToListAsync();
            return Json(ListResponse.Of(posts));
        }

        private static dynamic SelectPostView(Post p)
        {
            return new
            {
                p.Id, p.CreatedAt, p.Text, p.User.UserName, p.UserId, p.IsOriginalLocation,
                Likes = p.Likes.Count, Shares = 0, Comments = p.Comments.Count,
                Location = MapConvert.ToLatLon(p.Location)
            };
        }

        #endregion

        #region New post

        public class NewPostRequest
        {
            [Required] public string Text { get; set; } = string.Empty;
            [Required] public Point Location { get; set; } = default!;
            public bool IsFakeLocation { get; set; } = false;
        }

        public class NewPostResponse
        {
            public Post Post { get; set; } = default!;
        }

        [HttpPost]
        public async Task<ActionResult<NewPostResponse>> CreateNewPost([FromBody] NewPostRequest newPost)
        {
            var id = UserId;
            if (id == null)
                return Unauthorized();
            var post = await _postService.CreateTextPost(
                newPost.Text,
                id,
                newPost.Location,
                !newPost.IsFakeLocation);
            return new NewPostResponse
            {
                Post = post
            };
        }

        #endregion

        #region Update post

        public class UpdatePostRequest
        {
            [Required] public string Text { get; set; } = string.Empty;
        }

        public class UpdatePostResponse : NewPostResponse
        {
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<UpdatePostResponse>> UpdatePost([FromRoute] string id,
            [FromBody] UpdatePostRequest body)
        {
            var post = await _postService.GetPostOrNull(id);
            if (post == null)
                return NotFound();
            if (post.UserId == UserId)
            {
                post.Text = body.Text;
                post.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return new UpdatePostResponse
                {
                    Post = post
                };
            }

            return Unauthorized();
        }

        #endregion

        #region Comments

        [HttpGet("{id}/comments")]
        public async Task<ActionResult<ListResponse<PostCommentView>>> GetComments(
            [FromRoute] string id,
            [FromServices] ICommentService commentService,
            [FromQuery] DateTime? before = null)
        {
            if (!await _postService.IsAvailable(id))
                return NotFound();

            return new ListResponse<PostCommentView>(
                await commentService.QueryCommentViews(id, before, 30).ToListAsync()
            );
        }

        public class NewCommentRequest
        {
            [Required] public string Text { get; set; } = string.Empty;
            [Required] public string PostId { get; set; } = string.Empty;
            public long? ReplyTo { get; set; }
        }

        [HttpPost("{id}/comments")]
        public async Task<ActionResult<PostComment>> CreateComment(
            [FromRoute] string id,
            [FromServices] ICommentService commentService,
            [FromBody] NewCommentRequest body)
        {
            if (!await _postService.IsAvailable(id))
                return NotFound($"Post with id={id} does not exist or not available at the moment");

            var userId = UserId;
            if (userId == null)
                return Unauthorized("User id is not present in the token");

            PostComment comment;
            if (body.ReplyTo == null)
                comment = await commentService.CreateComment(body.PostId, userId, body.Text);
            else
                comment = await commentService.ReplyToComment((long)body.ReplyTo, userId, body.Text);

            return comment;
        }

        public class UpdateCommentRequest
        {
            [Required] public string Text { get; set; } = string.Empty;
        }

        [HttpPost("{id}/comments/{commentId:long}")]
        public async Task<ActionResult<PostComment>> UpdateComment(
            [FromRoute] string id,
            [FromRoute] long commentId,
            [FromServices] ICommentService commentService,
            [FromBody] UpdateCommentRequest body)
        {
            if (!await _postService.IsAvailable(id)) return NotFound($"Post with id={id} does not exist");

            var userId = UserId;
            if (userId == null)
                return Unauthorized("User id is not present in the token");

            var comment = await commentService.GetCommentOrNull(commentId);

            if (comment == null) return NotFound($"Comment with id={commentId} does not exist");

            if (comment.SenderId != userId) return Forbid("You can't update this comment");

            comment.Text = body.Text;
            await _context.SaveChangesAsync();
            return comment;
        }

        #endregion
    }
}