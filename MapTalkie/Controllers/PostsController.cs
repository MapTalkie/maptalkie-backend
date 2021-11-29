using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.DB;
using MapTalkie.DB.Context;
using MapTalkie.Domain.Messages.Posts;
using MapTalkie.Domain.Utils;
using MapTalkie.Domain.Utils.JsonConverters;
using MapTalkie.Services.PopularityProvider;
using MapTalkie.Utils;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace MapTalkie.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PostsController : AuthorizedController
    {
        private readonly AppDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public PostsController(AppDbContext dbContext, IPublishEndpoint publishEndpoint) : base(dbContext)
        {
            _context = dbContext;
            _publishEndpoint = publishEndpoint;
        }

        #region Получить пост(ы)

        public record PostView
        {
            public string UserId { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public string Avatar { get; set; } = string.Empty;

            [JsonConverter(typeof(IdToStringConverter))]
            public long Id { get; set; }

            public int Comments { get; set; }
            public int Shares { get; set; }
            public int Likes { get; set; }
            public double Rank { get; set; }
            public DateTime CreatedAt { get; set; }
            public string Text { get; set; } = string.Empty;
            public Point Location { get; set; } = new(0, 0) { SRID = 4326 };
        }

        [HttpGet("{postId}", Name = "GetPost")]
        public async Task<ActionResult<PostView>> GetPost([FromRoute] long postId)
        {
            var post = await _context.Posts
                .Where(p => p.Available && p.Id == postId)
                .Select(p => new PostView
                {
                    Id = p.Id,
                    Text = p.Text,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    UserName = p.User.UserName,
                    Avatar = "",
                    Location = p.Location,
                    Likes = p.CachedLikesCount + p.Likes.Count(l => l.CreatedAt > p.CacheUpdatedAt),
                    Shares = p.CachedSharesCount + p.Shares.Count(l => l.CreatedAt > p.CacheUpdatedAt),
                    Comments = p.CachedCommentsCount + p.Comments.Count(l => l.CreatedAt > p.CacheUpdatedAt),
                })
                .FirstOrDefaultAsync();
            if (post != null)
                return post;
            return NotFound();
        }

        [HttpGet("popular/{polygon}")]
        public async Task<ActionResult<List<PostView>>> GetPopularPosts(
            Polygon polygon,
            [FromServices] IPopularityProvider popularityProvider)
        {
            polygon = MapConvert.ToMercator(polygon);
            return await _context.Posts
                .Where(p => p.Available && polygon.Contains(p.Location))
                .Select(p => new PostView
                {
                    Id = p.Id,
                    Text = p.Text,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    UserName = p.User.UserName,
                    Avatar = "",
                    Location = p.Location,
                    Likes = p.CachedLikesCount + p.Likes.Count(l => l.CreatedAt > p.CacheUpdatedAt),
                    Shares = p.CachedSharesCount + p.Shares.Count(l => l.CreatedAt > p.CacheUpdatedAt),
                    Comments = p.CachedCommentsCount + p.Comments.Count(l => l.CreatedAt > p.CacheUpdatedAt),
                })
                .ToListAsync();
        }

        #endregion

        #region Новый пост

        public record NewPostPayload
        {
            [Required] public string Text { get; set; } = string.Empty;
            [Required] public Point Location { get; set; } = default!;
            public bool IsFakeLocation { get; set; } = false;
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewPost([FromBody] NewPostPayload newPost)
        {
            var userId = UserId;
            if (userId == null)
                return Unauthorized();
            var post = new Post
            {
                Text = newPost.Text,
                UserId = userId,
                Location = newPost.Location,
                IsOriginalLocation = !newPost.IsFakeLocation
            };
            _context.Add(post);
            await _context.SaveChangesAsync();
            await _publishEndpoint.Publish(new PostCreated
            {
                CreatedAt = post.CreatedAt,
                Location = post.Location,
                PostId = post.Id,
                UserId = post.UserId
            });
            return Created(
                Url.RouteUrl("GetPost", new { post.Id }),
                new { Id = post.Id.ToString() });
        }

        #endregion

        #region Обновить/удалить пост

        public class UpdatePostBody
        {
            [Required] public string Text { get; set; } = string.Empty;
        }

        [HttpPatch("{postId}")]
        public async Task<IActionResult> UpdatePost([FromRoute] long postId,
            [FromBody] UpdatePostBody body)
        {
            var post = await _context.Posts.Where(p => p.Available && p.Id == postId).FirstOrDefaultAsync();
            if (post == null)
                return NotFound();
            if (post.UserId == UserId)
            {
                post.Text = body.Text;
                post.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return NoContent();
            }

            return Unauthorized();
        }

        [HttpPatch("{postId}")]
        public async Task<IActionResult> DeletePost([FromRoute] long postId)
        {
            var post = await _context.Posts.Where(p => p.Available && p.Id == postId).FirstOrDefaultAsync();
            if (post == null)
                return NotFound();

            if (post.UserId != UserId)
                return Unauthorized();

            post.Available = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        #endregion

        #region Комментарии

        public record CommentView
        {
            public string UserName { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }

            [JsonConverter(typeof(IdToStringConverter))]
            public long Id { get; set; }

            [JsonConverter(typeof(IdToStringConverter))]
            public long PostId { get; set; }

            public string Text { get; set; } = string.Empty;
        }

        [HttpGet("{postId}/comments")]
        public async Task<ActionResult<ListResponse<CommentView>>> GetComments(
            [FromRoute] long postId,
            [FromQuery] DateTime? before = null)
        {
            if (!await _context.Posts.AnyAsync(p => p.Available && p.Id == postId))
                return NotFound();

            return new ListResponse<CommentView>(
                await _context.PostComments
                    .Where(c => c.Available && c.PostId == postId)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => SelectCommentView(c))
                    .ToListAsync()
            );
        }

        private static CommentView SelectCommentView(PostComment c)
        {
            return new CommentView
            {
                Id = c.Id,
                PostId = c.PostId,
                CreatedAt = c.CreatedAt,
                UserId = c.SenderId,
                UserName = c.Sender.Id
            };
        }

        public class NewCommentRequest
        {
            [Required] public string Text { get; set; } = string.Empty;
            public long? ReplyTo { get; set; }
        }

        [HttpPost("{postId}/comments")]
        public async Task<IActionResult> CreateComment(
            [FromRoute] long postId,
            [FromBody] NewCommentRequest body)
        {
            if (!await _context.Posts.AnyAsync(p => p.Id == postId && p.Available))
                return NotFound($"Post with id={postId} does not exist or not available at the moment");

            var userId = UserId;
            if (userId == null)
                return Unauthorized("User id is not present in the token");

            var comment = new PostComment
            {
                SenderId = userId,
                Text = body.Text,
            };

            if (body.ReplyTo != null)
            {
                if (!await _context.PostComments
                    .AnyAsync(c => c.PostId == postId && c.Id == body.ReplyTo && c.Available))
                    return NotFound($"Comment with id={body.ReplyTo} not found or belongs to a different post");
            }

            return Ok();
        }

        public class UpdateCommentRequest
        {
            [Required] public string Text { get; set; } = string.Empty;
        }

        [HttpPost("{postId}/comments/{commentId:long}")]
        public async Task<ActionResult<PostComment>> UpdateComment(
            [FromRoute] long postId,
            [FromRoute] long commentId,
            [FromBody] UpdateCommentRequest body)
        {
            var userId = UserId;
            if (userId == null)
                return Unauthorized("User id is not present in the token");

            if (!await _context.Posts.AnyAsync(p => p.Id == postId && p.Available))
                return NotFound($"Post with id={postId} does not exist");

            var comment = await _context.PostComments.FirstOrDefaultAsync(
                p => p.PostId == postId && p.Id == commentId && p.Available);

            if (comment == null) return NotFound($"Comment with id={commentId} does not exist");
            if (comment.SenderId != userId) return Forbid("You can't update this comment");
            comment.Text = body.Text;
            comment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return comment;
        }

        [HttpDelete("{postId}/comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(long commentId, long postId)
        {
            var comment = await _context.PostComments
                .Where(c => c.Id == commentId && c.PostId == postId && c.Available)
                .FirstOrDefaultAsync();

            if (comment == null)
                return NotFound("Comment not found");
            comment.Available = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        #endregion
    }
}