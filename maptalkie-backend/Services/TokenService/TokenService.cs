using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MapTalkie.Configuration;
using MapTalkie.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MapTalkie.Services.TokenService
{
    public class TokenService : ITokenService
    {
        private JwtSecurityTokenHandler _tokenHandler = new();
        private JwtSettings _jwtSettings;

        public TokenService(JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
        }

        public JwtSecurityToken GenerateToken(User user, MapTalkieTokenOptions options)
        {
            var desc = MakeTokenDescriptor(user, options);
            return _tokenHandler.CreateJwtSecurityToken(desc);
        }

        public JwtTokenResult CreateToken(User user, MapTalkieTokenOptions options)
        {
            var token = GenerateToken(user, options);
            var tokenString = _tokenHandler.WriteToken(token);
            return new JwtTokenResult(tokenString);
        }

        public JwtTokenResult CreateToken(User user)
        {
            return CreateToken(user, new MapTalkieTokenOptions
            {
                Lifetime = _jwtSettings.TokenLifeTime
            });
        }

        private SecurityTokenDescriptor MakeTokenDescriptor(User user, MapTalkieTokenOptions options)
        {
            return new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id)
                }),
                Expires = DateTime.Now.Add(options.Lifetime),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience.IsNullOrEmpty() ? null : _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(_jwtSettings.GetSecurityKey(), SecurityAlgorithms.HmacSha256Signature)
            };
        }
    }
}