using System.IdentityModel.Tokens.Jwt;
using MapTalkie.DB;

namespace MapTalkie.Services.TokenService
{
    public interface ITokenService
    {
        JwtSecurityToken GenerateToken(User user, MapTalkieTokenOptions options);

        JwtTokenResult CreateToken(User user, MapTalkieTokenOptions options);

        JwtTokenResult CreateToken(User user);
    }
}