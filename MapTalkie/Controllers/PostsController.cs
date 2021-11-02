using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MapTalkie.Models;
using MapTalkie.Models.Context;
using MapTalkie.Services.CommentService;
using MapTalkie.Services.PostService;
using MapTalkie.Utils;
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
            AppDbContext context,
            IPostService postService,
            UserManager<User> userManager) : base(userManager)
        {
            _postService = postService;
            _context = context;
        }

        #region Delete post

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DeletePost([FromRoute] long id)
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

        #region Get post

        [HttpGet("{id:long}")]
        public async Task<ActionResult<MapPost>> GetPost([FromRoute] long id)
        {
            var post = await _postService.GetPostOrNull(id);
            if (post != null)
                return post;
            return NotFound();
        }

        [HttpGet("geo/{polygon}")]
        public async Task<ListResponse<MapPost>> FindPostsInArea([FromRoute] Polygon polygon)
        {
            return new ListResponse<MapPost>(
                await _postService.QueryPostsInArea(polygon).ToListAsync());
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
            public MapPost Post { get; set; } = default!;
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

        [HttpPatch("{id:long}")]
        public async Task<ActionResult<UpdatePostResponse>> UpdatePost([FromRoute] long id,
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

        [HttpGet("{id:long}/comments")]
        public async Task<ActionResult<ListResponse<PostCommentView>>> GetComments(
            [FromRoute] long id,
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
            public string Text { get; set; } = string.Empty;
            public long PostId { get; set; }
            public long? ReplyTo { get; set; }
        }

        [HttpPost("{id:long}/comments")]
        public async Task<ActionResult<PostComment>> CreateComment(
            [FromRoute] long id,
            [FromServices] ICommentService commentService,
            [FromBody] NewCommentRequest body)
        {
            if (!await _postService.IsAvailable(id))
            {
                return NotFound($"Post with id={id} does not exist or not available at the moment");
            }

            var userId = UserId;
            if (userId == null)
                return Unauthorized("User id is not present in the token");

            PostComment comment;
            if (body.ReplyTo == null)
            {
                comment = await commentService.CreateComment(body.PostId, userId, body.Text);
            }
            else
            {
                comment = await commentService.ReplyToComment((long)body.ReplyTo, userId, body.Text);
            }

            return comment;
        }

        public class UpdateCommentRequest
        {
            [Required] public string Text { get; set; } = string.Empty;
        }

        [HttpPost("{id:long}/comments/{commentId:long}")]
        public async Task<ActionResult<PostComment>> UpdateComment(
            [FromRoute] long id,
            [FromRoute] long commentId,
            [FromServices] ICommentService commentService,
            [FromBody] UpdateCommentRequest body)
        {
            if (!await _postService.IsAvailable(id))
            {
                return NotFound($"Post with id={id} does not exist");
            }

            var userId = UserId;
            if (userId == null)
                return Unauthorized("User id is not present in the token");

            var comment = await commentService.GetCommentOrNull(commentId);

            if (comment == null)
            {
                return NotFound($"Comment with id={commentId} does not exist");
            }

            if (comment.SenderId != userId)
            {
                return Forbid("You can't update this comment");
            }

            comment.Text = body.Text;
            await _context.SaveChangesAsync();
            return comment;
        }

        #endregion
    }
}