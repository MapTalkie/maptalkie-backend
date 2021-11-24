using Microsoft.Extensions.Configuration;

namespace MapTalkie.Configuration
{
    public static class ConfigurationExtensions
    {
        internal static T GetSection<T>(this IConfiguration configuration)
        {
            return configuration.GetSection(typeof(T).Name).Get<T>();
        }
    }
}