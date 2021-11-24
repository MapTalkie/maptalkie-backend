using System;
using System.Net;
using System.Threading.Tasks;
using MapTalkie.DB;
using MapTalkie.Utils.ErrorHandling;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace MapTalkie.Hubs
{
    public class AuthorizedHub : Hub
    {
        private readonly UserManager<User> _userManager;
        private User? _user;
        private DateTime _userCachedAt = DateTime.MinValue;
        protected TimeSpan UserCacheDuration = TimeSpan.FromMinutes(1);

        public AuthorizedHub(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        private string? UserIdPrivate
        {
            get
            {
                try
                {
                    return _userManager.GetUserId(Context.User);
                }
                catch
                {
                    return null;
                }
            }
        }

        protected string UserId => UserIdPrivate ?? throw new HttpException(HttpStatusCode.Unauthorized);

        protected async Task<User> GetUser()
        {
            if (_user == null || _userCachedAt + UserCacheDuration < DateTime.Now)
            {
                _user = await GetUserImpl();
                _userCachedAt = DateTime.Now;
            }

            return _user;
        }

        private Task<User> GetUserImpl()
        {
            return _userManager.FindByIdAsync(UserId);
        }
    }
}