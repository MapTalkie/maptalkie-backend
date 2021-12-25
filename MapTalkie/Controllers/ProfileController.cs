using System.Threading.Tasks;
using MapTalkie.DB.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MapTalkie.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class ProfileController : AuthorizedController
{
    private readonly AppDbContext _context;

    public ProfileController(AppDbContext context) : base(context)
    {
        _context = context;
    }

    #region Get user

    [HttpGet]
    public async Task<ActionResult<dynamic>> GetCurrenUser()
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
    public async Task<ActionResult<dynamic>> Update([FromBody] UpdateUserProfile body)
    {
        var user = await GetUser();
        if (user == null)
            return Unauthorized();

        if (body.NonFriendMessages != null)
            user.AllowsNonFriendMessages = (bool)body.NonFriendMessages;
        if (body.PrivateLocation != null)
            user.UsesPrivateLocation = (bool)body.PrivateLocation;
        await _context.SaveChangesAsync();
        return new { };
    }

    #endregion
}