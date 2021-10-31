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

        public AuthorizedController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public int? UserId
        {
            get
            {
                var claim = HttpContext.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier);
                if (claim == null)
                    return null;
                if (int.TryParse(claim.Value, out var value))
                    return value;
                return null;
            }
        }

        protected Task<User?> GetUser()
        {
            var id = UserId;
            return id == null ? Task.FromResult<User?>(null) : _userManager.FindByIdAsync(id);
        }
    }
}