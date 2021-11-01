using System.Security.Claims;
using System.Threading.Tasks;
using MapTalkie.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MapTalkie.Controllers
{
    public class AuthorizedController : Controller
    {
        private readonly UserManager<User> _userManager;
        private User? _user;
        private bool _userInitialized = false;

        public AuthorizedController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public string? UserId => HttpContext.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        protected async Task<User?> GetUser()
        {
            if (_userInitialized) return _user;
            _user = await GetUserPrivate();
            _userInitialized = true;
            return _user;
        }

        private Task<User?> GetUserPrivate()
        {
            var id = UserId;
            if (id == null)
                return Task.FromResult<User?>(null);
            return _userManager.FindByIdAsync(id)!;
        }
    }
}