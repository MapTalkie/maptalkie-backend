using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Configuration;
using MapTalkie.DB;
using MapTalkie.DB.Context;
using Microsoft.Extensions.Options;

namespace MapTalkie.Services.AuthService
{
    public class AuthService : DbService, IAuthService
    {
        private readonly IOptions<AuthenticationSettings> _authenticationSettings;

        public AuthService(AppDbContext context, IOptions<AuthenticationSettings> authenticationSettings) :
            base(context)
        {
            _authenticationSettings = authenticationSettings;
        }

        public Task<IRefreshTokenResult> CreateRefreshToken(User user) => CreateRefreshToken(user.Id);

        public Task<RefreshToken?> FindRefreshToken(string token)
        {
            return DbContext.RefreshTokens
                .Where(t => t.Id == token && !t.IsBlocked && t.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync()!;
        }

        public async Task<IRefreshTokenResult> RotateToken(RefreshToken refreshToken)
        {
            var token = await CreateRefreshToken(refreshToken.UserId);
            DbContext.RefreshTokens.Remove(refreshToken);
            await DbContext.SaveChangesAsync();
            return token;
        }

        public async Task<IRefreshTokenResult> CreateRefreshToken(string userId)
        {
            var token = new RefreshToken
            {
                UserId = userId,
                ExpiresAt = DateTime.UtcNow + _authenticationSettings.Value.RefreshTokenLifetime
            };
            DbContext.RefreshTokens.Add(token);
            await DbContext.SaveChangesAsync();
            return new RefreshTokenResult(token.Id, token);
        }

        private class RefreshTokenResult : IRefreshTokenResult
        {
            public RefreshTokenResult(string token, RefreshToken model)
            {
                Model = model;
                Token = token;
            }

            public string Token { get; }
            public RefreshToken Model { get; }
        }
    }
}