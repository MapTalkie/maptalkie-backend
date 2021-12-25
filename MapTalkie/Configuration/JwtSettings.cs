using System;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MapTalkie.Configuration;

public class JwtSettings
{
    public string Issuer { get; set; } = string.Empty;

    public TimeSpan TokenLifeTime { get; set; } = TimeSpan.FromDays(2);

    public TimeSpan RefreshTokenLifeTime { get; set; } = TimeSpan.FromDays(14);

    public string SecretKey { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public bool ValidateAudience { get; set; } = true;

    internal SecurityKey GetSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
    }
}