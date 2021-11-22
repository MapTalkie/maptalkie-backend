using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using MapTalkie.Utils.ErrorHandling;
using MapTalkieDB;
using MapTalkieDB.Context;
using Microsoft.AspNetCore.Mvc;

namespace MapTalkie.Controllers
{
    public class AuthorizedController : Controller
    {
        protected readonly AppDbContext _dbContext;
        private User? _user;
        private bool _userInitialized;

        public AuthorizedController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
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
            return _dbContext.Users.Where(u => u.Id == id).FirstOrDefaultAsync()!;
        }
    }
}