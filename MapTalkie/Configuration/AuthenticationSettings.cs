using System;

namespace MapTalkie.Configuration;

public class AuthenticationSettings
{
    public string HybridCookieName { get; set; } = "jwt.sig";
    public string RefreshTokenCookieName { get; set; } = "refresh";
    public TimeSpan RefreshTokenLifetime { get; set; } = TimeSpan.FromDays(90);
}