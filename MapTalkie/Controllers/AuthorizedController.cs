using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using MapTalkie.DB;
using MapTalkie.DB.Context;
using MapTalkie.Utils.ErrorHandling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Controllers
{
    public class AuthorizedController : Controller
    {
        protected readonly AppDbContext _context;
        private User? _user;
        private bool _userInitialized;

        public AuthorizedController(AppDbContext context)
        {
            _context = context;
        }

        public string? UserId => HttpContext.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        protected string RequireUserId()
        {
            var userId = UserId;
            if (userId == null)
                throw new HttpException(HttpStatusCode.Unauthorized);
            return userId;
        }

        protected async Task<User?> GetUser()
        {
            if (_userInitialized) return _user;
            _user = await GetUserPrivate();
            _userInitialized = true;
            return _user;
        }

        protected async Task<User> RequireUser()
        {
            var user = await GetUser();
            if (user == null)
                throw new HttpException(HttpStatusCode.Unauthorized);
            return user;
        }

        private Task<User?> GetUserPrivate()
        {
            var id = UserId;
            if (id == null)
                return Task.FromResult<User?>(null);
            return _context.Users.Where(u => u.Id == id).FirstOrDefaultAsync()!;
        }
    }
}