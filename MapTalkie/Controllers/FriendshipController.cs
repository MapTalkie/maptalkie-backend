using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.DB.Context;
using MapTalkie.Services.FriendshipService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FriendshipController : AuthorizedController
    {
        private readonly AppDbContext _context;

        public FriendshipController(AppDbContext context) : base(context)
        {
            _context = context;
        }

        [HttpGet("/friends")]
        public Task<List<FriendView>> GetFriends([FromServices] IFriendshipService friendshipService)
        {
            var userId = RequireUserId();
            return _context.FriendRequests
                .Where(r => r.FromId == userId)
                .Select(r => new FriendView
                {
                    UserName = r.To.UserName,
                    UserId = r.ToId,
                    IsMutual = _context.FriendRequests.Any(r2 => r2.ToId == userId && r2.FromId == r.ToId)
                })
                .ToListAsync();
        }

        public record FriendView
        {
            public string UserName { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
            public bool IsMutual { get; set; }
        }
    }
}