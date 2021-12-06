using System.Threading.Tasks;
using MapTalkie.DB.Context;
using MapTalkie.Services.FriendshipService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Controllers
{
    [Authorize, ApiController, Route("api/[controller]")]
    public class FriendshipController : AuthorizedController
    {
        private readonly IFriendshipService _friendshipService;

        public FriendshipController(AppDbContext context, IFriendshipService friendshipService) : base(context)
        {
            _friendshipService = friendshipService;
        }

        [HttpGet("/friends")]
        public Task<FriendshipsView> GetFriends([FromServices] IFriendshipService friendshipService)
        {
            return _friendshipService.FindFriendships(RequireUserId());
        }

        [HttpPost("/friendship/{userId}")]
        public async Task<IActionResult> RequestFriendship(string userId)
        {
            if (userId == RequireUserId())
                return Forbid("You can't become your own friend, sorry about that");
            if (!await _context.Users.AnyAsync(u => u.Id == userId))
                return NotFound();
            await _friendshipService.EnsureFriendshipRequest(RequireUserId(), userId);
            return Ok();
        }

        [HttpDelete("/friendship/{userId}")]
        public async Task<IActionResult> RevokeFriendship(string userId)
        {
            if (userId == RequireUserId())
                return Forbid(
                    "There's something metaphorical about not being a friend of yourself, but no, you can't do that");
            if (!await _context.Users.AnyAsync(u => u.Id == userId))
                return NotFound();
            await _friendshipService.RevokeFriendship(RequireUserId(), userId);
            return Ok();
        }
    }
}