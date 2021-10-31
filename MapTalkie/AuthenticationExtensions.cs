using System.Threading.Tasks;
using MapTalkie.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace MapTalkie
{
    public static class AuthenticationExtensions
    {
        internal static AuthenticationBuilder AddMapTalkieJwtBearer(
            this AuthenticationBuilder services,
            JwtSettings jwtSettings,
            string hybridAuthenticationCookie,
            string schemaName)
        {
            return services.AddJwtBearer(schemaName, options =>
            {
                options.Events.OnMessageReceived = opts => TryHybridJwtAuthentication(opts, hybridAuthenticationCookie);
                options.IncludeErrorDetails = true;
                options.ClaimsIssuer = jwtSettings.Issuer;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,

                    ValidateLifetime = true,

                    ValidAudience = jwtSettings.Audience,
                    ValidateAudience = jwtSettings.ValidateAudience,

                    IssuerSigningKey = jwtSettings.GetSecurityKey(),
                    ValidateIssuerSigningKey = true,
                };
            });
        }

        private static Task TryHybridJwtAuthentication(MessageReceivedContext context, string cookieName)
        {
            var authorization = context.Request.Headers[HeaderNames.Authorization].ToString();

            if (!authorization.StartsWith("Bearer ")) return Task.CompletedTask;
            if (!context.Request.Cookies.ContainsKey(cookieName)) return Task.CompletedTask;

            var jwtSig = context.Request.Cookies[cookieName]!;
            if (jwtSig.Length != 0)
                context.Token = authorization["Bearer ".Length..] + "." + jwtSig;
            return Task.CompletedTask;
        }
    }
}