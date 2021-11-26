using System.Threading.Tasks;
using MapTalkie.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

namespace MapTalkie
{
    public static class AuthenticationExtensions
    {
        private const string HybridAuthenticationSchema = "HybridBearer";

        public static void AddAppAuthorization(
            this IServiceCollection services,
            JwtSettings jwtSettings,
            AuthenticationSettings authenticationSettings)
        {
            var auth = services.AddAuthentication();

            auth.AddJwtBearer(options => { SetupJwtBearerCommonOptions(options, jwtSettings); });

            auth.AddJwtBearer(HybridAuthenticationSchema, options =>
            {
                SetupJwtBearerCommonOptions(options, jwtSettings);
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = opts =>
                        TryHybridJwtAuthentication(opts, authenticationSettings.HybridCookieName)
                };
            });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, HybridAuthenticationSchema)
                    .Build();
            });
        }

        public static void AddAppAuthorization(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAppAuthorization(
                configuration.GetSection<JwtSettings>(),
                configuration.GetSection<AuthenticationSettings>());
        }

        private static void SetupJwtBearerCommonOptions(JwtBearerOptions options, JwtSettings jwtSettings)
        {
            options.IncludeErrorDetails = true;
            options.ClaimsIssuer = jwtSettings.Issuer;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,

                ValidateLifetime = true,

                ValidAudience = jwtSettings.Audience,
                ValidateAudience = jwtSettings.ValidateAudience,

                IssuerSigningKey = jwtSettings.GetSecurityKey(),
                ValidateIssuerSigningKey = true
            };
        }

        private static Task TryHybridJwtAuthentication(MessageReceivedContext context, string cookieName)
        {
            var authorization = context.Request.Headers[HeaderNames.Authorization].ToString();
            context.Request.Cookies.TryGetValue(cookieName, out var jwtSig);

            if (authorization == string.Empty && jwtSig != null && context.Request.Query.ContainsKey("access_token"))
            {
                context.Token = context.Request.Query["access_token"] + "." + jwtSig;
                return Task.CompletedTask;
            }

            if (!authorization.StartsWith("Bearer ")) return Task.CompletedTask;
            if (!context.Request.Cookies.ContainsKey(cookieName)) return Task.CompletedTask;

            if (jwtSig != null)
                context.Token = authorization["Bearer ".Length..] + "." + jwtSig;
            return Task.CompletedTask;
        }
    }
}