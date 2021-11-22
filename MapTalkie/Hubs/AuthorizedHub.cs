using System;
using System.Net;
using System.Threading.Tasks;
using MapTalkie.Utils.ErrorHandling;
using MapTalkieDB;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace MapTalkie.Hubs
{
    public class AuthorizedHub : Hub
    {
        private readonly DateTime _userCachedAt = DateTime.MinValue;
        private readonly UserManager<User> _userManager;
        private User? _user;
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
            if (_user == null || _userCachedAt + UserCacheDuration < DateTime.Now) _user = await GetUserImpl();

            return _user;
        }

        private Task<User> GetUserImpl()
        {
            return _userManager.FindByIdAsync(UserId);
        }
    }
}