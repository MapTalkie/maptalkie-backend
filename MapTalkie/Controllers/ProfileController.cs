using System.Threading.Tasks;
using MapTalkie.Models;
using MapTalkie.Models.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MapTalkie.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ProfileController : AuthorizedController
    {
        private readonly AppDbContext _context;

        public ProfileController(UserManager<User> userManager, AppDbContext context) : base(userManager)
        {
            _context = context;
        }

        #region Get user

        [HttpGet]
        public async Task<ActionResult<User>> GetCurrenUser()
        {
            var user = await GetUser();
            if (user == null)
                return Unauthorized();
            return user;
        }

        #endregion

        #region Update profile

        public class UpdateUserProfile
        {
            public bool? PrivateLocation { get; set; }
            public bool? NonFriendMessages { get; set; }
        }

        [HttpPatch]
        public async Task<ActionResult<User>> Update([FromBody] UpdateUserProfile body)
        {
            var user = await GetUser();
            if (user == null)
                return Unauthorized();

            if (body.NonFriendMessages != null)
                user.Settings.Privacy.NonFriendMessages = (bool)body.NonFriendMessages;
            if (body.PrivateLocation != null)
                user.Settings.Privacy.PrivateLocation = (bool)body.PrivateLocation;
            await _context.SaveChangesAsync();
            return user;
        }

        #endregion
    }
}