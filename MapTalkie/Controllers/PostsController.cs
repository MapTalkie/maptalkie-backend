using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.DB;
using MapTalkie.DB.Context;
using MapTalkie.Domain.Messages.Posts;
using MapTalkie.Domain.Popularity;
using MapTalkie.Domain.Utils;
using MapTalkie.Utils;
using MapTalkie.Views;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace MapTalkie.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : AuthorizedController
    {
        private readonly AppDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public PostsController(
            AppDbContext context,
            IPublishEndpoint publishEndpoint) : base(context)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        #region Получить пост(ы)

        [HttpGet("{postId}", Name = "GetPost")]
        public async Task<ActionResult<PostView>> GetPost([FromRoute] long postId)
        {
            var post = await _context.Posts
                .Where(p => p.Available && p.Id == postId)
                .Select(PostView.Projection)
                .FirstOrDefaultAsync();
            if (post != null)
                return post;
            return NotFound();
        }

        [HttpGet("popular/{polygon}")]
        public async Task<ActionResult<List<PostView>>> GetPopularPosts(Polygon polygon, int limit = 50)
        {
            limit = Math.Min(20, Math.Max(limit, 100));
            polygon = MapConvert.ToMercator(polygon);

            var query = _context.Posts
                .Where(p => p.Available && polygon.Contains(p.Location))
                .OrderByDescending(Popularity.PopularityRankProjection);
            var posts = await query.Select(PostView.Projection).Take(limit).ToListAsync();
            return posts;
        }

        [HttpGet("latest/{polygon}")]
        public async Task<ActionResult<List<PostView>>> GetLatestPosts(Polygon polygon, int limit = 50)
        {
            limit = Math.Min(20, Math.Max(limit, 100));
            polygon = MapConvert.ToMercator(polygon);

            var query = _context.Posts
                .Where(p => p.Available && polygon.Contains(p.Location))
                .OrderByDescending(p => p.CreatedAt);
            var posts = await query.Select(PostView.Projection).Take(limit).ToListAsync();
            return posts;
        }

        #endregion

        #region Новый пост

        public record NewPostPayload
        {
            [Required] public string Text { get; set; } = string.Empty;
            [Required] public Point Location { get; set; } = default!;
            public bool IsOriginalLocation { get; set; } = false;
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> CreateNewPost([FromBody] NewPostPayload newPost)
        {
            var userId = UserId;
            if (userId == null)
                return Unauthorized();
            var user = await RequireUser();
            var post = new Post
            {
                UserId = user.Id,
                IsOriginalLocation = newPost.IsOriginalLocation,
                Location = MapConvert.ToMercator(newPost.Location),
                Text = newPost.Text
            };
            _context.Add(post);
            await _context.SaveChangesAsync();
            await _publishEndpoint.Publish(new PostCreated(post.CreatedAt, post.Id, post.UserId, post.Location));
            return Created(
                Url.RouteUrl("GetPost", new { postId = post.Id })!,
                new { Id = post.Id.ToString() });
        }

        #endregion

        #region Обновить/удалить пост

        public class UpdatePostBody
        {
            [Required] public string Text { get; set; } = string.Empty;
        }

        [HttpPatch("{postId}"), Authorize]
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

        [HttpDelete("{postId}"), Authorize]
        public async Task<IActionResult> DeletePost([FromRoute] long postId)
        {
            var post = await _context.Posts.Where(p => p.Available && p.Id == postId).FirstOrDefaultAsync();
            if (post == null)
                return NotFound();

            if (post.UserId != UserId)
                return Unauthorized();

            post.Available = false;
            await _context.SaveChangesAsync();
            await _publishEndpoint.Publish(new PostDeleted(post.Id, post.UserId, post.Location));
            return NoContent();
        }

        #endregion

        #region Комментарии

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
                    .Select(CommentView.Projection)
                    .ToListAsync()
            );
        }

        public class NewCommentRequest
        {
            [Required] public string Text { get; set; } = string.Empty;
            public long? ReplyTo { get; set; }
        }

        [HttpPost("{postId}/comments"), Authorize]
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
                Text = body.Text
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

        [HttpPost("{postId}/comments/{commentId:long}"), Authorize]
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

        [HttpDelete("{postId}/comments/{commentId}"), Authorize]
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

        #region Лайки

        [HttpPut("{postId}/like"), Authorize]
        public async Task<IActionResult> LikePost(long postId)
        {
            var location = await _context.Posts
                .Where(p => p.Id == postId && p.Available).Select(p => p.Location)
                .FirstOrDefaultAsync();
            if (location == null)
                return NotFound();
            var userId = RequireUserId();
            if (!await _context.PostLikes.AnyAsync(l => l.UserId == userId && l.PostId == postId))
            {
                var like = new PostLike
                {
                    PostId = postId,
                    UserId = userId
                };
                _context.Add(like);
                await _context.SaveChangesAsync();
                await _publishEndpoint.Publish(
                    new PostEngagement(postId, userId, location, PostEngagementType.Favorite));
            }

            return Ok();
        }

        [HttpDelete("{postId}/like"), Authorize]
        public async Task<IActionResult> UnLikePost(long postId)
        {
            var location = await _context.Posts
                .Where(p => p.Id == postId && p.Available).Select(p => p.Location)
                .FirstOrDefaultAsync();
            if (location == null)
                return NotFound();
            var userId = RequireUserId();
            var like = await _context.PostLikes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
            if (like == null) return Ok();

            _context.Remove(like);
            await _context.SaveChangesAsync();
            await _publishEndpoint.Publish(new PostEngagement(postId, userId, location,
                PostEngagementType.FavoriteRemoved));

            return Ok();
        }

        #endregion
    }
}