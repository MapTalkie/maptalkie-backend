using System;
using MapTalkie.Configuration;
using MapTalkie.Models.Context;
using MapTalkie.Services.CommentService;
using MapTalkie.Services.FriendshipService;
using MapTalkie.Services.PostService;
using MapTalkie.Services.TokenService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;

namespace MapTalkie
{
    public static class StartupExtension
    {
        public static void AddAppServices(this IServiceCollection services)
        {
            services
                .AddScoped<ICommentService, CommentService>()
                .AddScoped<IFriendshipService, FriendshipService>()
                .AddScoped<ITokenService, TokenService>()
                .AddScoped<IPostService, PostService>();
        }

        public static void AddConfiguration(this IServiceCollection services)
        {
            services
                .AddSingleton(provider => provider.GetRequiredService<IConfiguration>().GetJwtSettings())
                .AddSingleton(provider => provider.GetRequiredService<IConfiguration>().GetAuthenticationSettings());
        }

        public static void AddAppCors(this IServiceCollection services, IWebHostEnvironment env)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    if (env.IsDevelopment())
                    {
                        builder.WithOrigins(
                            "http://localhost:3000", "https://localhost:3000"
                        );
                    }
                    else
                    {
                        // TODO
                        throw new NotImplementedException();
                    }

                    builder.AllowCredentials();
                    builder.WithHeaders(HeaderNames.Authorization, HeaderNames.Accept, HeaderNames.CacheControl);
                });
            });
        }

        public static void AddAppDbContext(this IServiceCollection services, IWebHostEnvironment env,
            IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("Default");
                if (env.IsDevelopment())
                    options.UseSqlite(connectionString, builder => builder.UseNetTopologySuite());
                else
                    options.UseNpgsql(connectionString, builder => builder.UseNetTopologySuite());
            });
        }
    }
}