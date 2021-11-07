using System;
using System.Collections.Generic;
using MapTalkie.Configuration;
using MapTalkie.Models.Context;
using MapTalkie.Services.CommentService;
using MapTalkie.Services.EventBus;
using MapTalkie.Services.FriendshipService;
using MapTalkie.Services.PostService;
using MapTalkie.Services.TokenService;
using MapTalkie.Utils.Binders;
using MapTalkie.Utils.JsonConverters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Quartz;

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

            services.AddSingleton<IEventBus, LocalEventBus>();
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
                            "http://localhost:3000", "https://localhost:3000",
                            "http://localhost:5500", "https://localhost:5500",
                            "http://127.0.0.1:3000", "https://127.0.0.1:3000",
                            "http://127.0.0.1:5500", "https://127.0.0.1:5500"
                        );
                    }
                    else
                    {
                        // TODO
                        throw new NotImplementedException();
                    }

                    builder.AllowCredentials();
                    builder.AllowAnyMethod();

                    builder.WithHeaders(
                        HeaderNames.ContentType,
                        HeaderNames.Authorization,
                        HeaderNames.Accept,
                        HeaderNames.XRequestedWith,
                        HeaderNames.CacheControl);
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

        public static void AddAppSignalR(this IServiceCollection collection)
        {
            collection
                .AddSignalR()
                .AddNewtonsoftJsonProtocol(options =>
                {
                    AddJsonConverters(options.PayloadSerializerSettings.Converters);
                });
        }

        public static void AddAppControllers(this IServiceCollection collection)
        {
            collection
                .AddControllers(options =>
                {
                    options.ModelBinderProviders.Add(
                        new AppModelBinderProvider());
                })
                .AddNewtonsoftJson(options => AddJsonConverters(options.SerializerSettings.Converters));
        }

        private static void AddJsonConverters(IList<JsonConverter> converters)
        {
            converters.Add(new PolygonJsonConverter());
            converters.Add(new PointJsonConverter());
        }

        public static void AddAppQuartz(this IServiceCollection services)
        {
            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();

                q.UseInMemoryStore();
                q.UseSimpleTypeLoader();
                q.UseDefaultThreadPool(tp => { tp.MaxConcurrency = 10; });
            });
            services.AddQuartzHostedService(options =>
            {
                // when shutting down we want jobs to complete gracefully
                options.WaitForJobsToComplete = true;
            });
        }
    }
}