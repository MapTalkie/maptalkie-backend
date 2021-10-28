using System;
using System.IdentityModel.Tokens.Jwt;
using MapTalkie.Models;

namespace MapTalkie.Services.TokenService
{
    public interface ITokenService
    {
        JwtSecurityToken GenerateToken(User user, MapTalkieTokenOptions options);

        JwtTokenResult CreateToken(User user, MapTalkieTokenOptions options);

        JwtTokenResult CreateToken(User user);
    }
}