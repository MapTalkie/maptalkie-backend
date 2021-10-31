using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MapTalkie.Models;
using MapTalkie.Services.PostService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MapTalkie.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PostsController : Controller
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpPost]
        public Task<ActionResult<NewPost>> CreateNewPost([FromBody] CreateNewPost newPost)
        {
            HttpContext.User
        }

        public class CreateNewPost
        {
            [Required] public string Text { get; set; } = string.Empty;
        }

        public class NewPost
        {
            public MapPost Post { get; set; } = default!;
        }
    }
}