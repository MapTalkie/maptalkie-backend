using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using MapTalkie.Models;
using MapTalkie.Utils.ErrorHandling;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MapTalkie.Controllers
{
    public class AuthorizedController : Controller
    {
        protected readonly UserManager<User> UserManager;
        private User? _user;
        private bool _userInitialized = false;

        public AuthorizedController(UserManager<User> userManager)
        {
            UserManager = userManager;
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
            return UserManager.FindByIdAsync(id)!;
        }
    }
}