using System.Threading.Tasks;
using MapTalkie.DB;

namespace MapTalkie.Services.AuthService;

public interface IRefreshTokenResult
{
    string Token { get; }
    RefreshToken Model { get; }
}

public interface IAuthService
{
    Task<IRefreshTokenResult> CreateRefreshToken(User user);
    Task<RefreshToken?> FindRefreshToken(string token);
    Task<IRefreshTokenResult> RotateToken(RefreshToken refreshToken);
}