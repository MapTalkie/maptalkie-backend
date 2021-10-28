using Microsoft.Extensions.Configuration;

namespace MapTalkie.Configuration
{
    public static class ConfigurationExtensions
    {
        internal static JwtSettings GetJwtSettings(this IConfiguration configuration)
        {
            return configuration.GetSection("JwtSettings").Get<JwtSettings>();
        }
        
        internal static AuthenticationSettings GetAuthenticationSettings(this IConfiguration configuration)
        {
            return configuration.GetSection("AuthenticationSettings").Get<AuthenticationSettings>();
        }
    }
}