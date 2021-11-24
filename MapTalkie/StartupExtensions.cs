using System;
using System.Collections.Generic;
using IdGen;
using MapTalkie.Configuration;
using MapTalkie.DB.Context;
using MapTalkie.Services.AuthService;
using MapTalkie.Services.CommentService;
using MapTalkie.Services.FriendshipService;
using MapTalkie.Services.MessageService;
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
                .AddScoped<IAuthService, AuthService>()
                .AddScoped<IFriendshipService, FriendshipService>()
                .AddScoped<ITokenService, TokenService>()
                .AddScoped<IMessageService, MessageService>()
                .AddScoped<IPostService, PostService>();
        }

        public static void ConfigureAll(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
            services.Configure<AuthenticationSettings>(configuration.GetSection(nameof(AuthenticationSettings)));
            services.Configure<PostOptions>(configuration.GetSection(nameof(PostOptions)));
        }

        public static void AddAppCors(this IServiceCollection services, IWebHostEnvironment env)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    if (env.IsDevelopment())
                        builder.WithOrigins(
                            "http://localhost:3000", "https://localhost:3000",
                            "http://localhost:5500", "https://localhost:5500",
                            "http://127.0.0.1:3000", "https://127.0.0.1:3000",
                            "http://127.0.0.1:5500", "https://127.0.0.1:5500"
                        );
                    else
                        // TODO
                        throw new NotImplementedException();

                    builder.AllowCredentials();
                    builder.AllowAnyMethod();

                    builder.WithHeaders(
                        "X-SignalR-User-Agent",
                        HeaderNames.ContentType,
                        HeaderNames.Authorization,
                        HeaderNames.Accept,
                        HeaderNames.XRequestedWith,
                        HeaderNames.CacheControl);
                });
            });
        }

        public static void AddAppDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            // TODO поменять
            services.AddSingleton(new IdGenerator(0));
            services.AddDbContext<AppDbContext>(options =>
            {
                string connectionString = configuration.GetConnectionString("Postgres");
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
                    options.ModelBinderProviders.Insert(
                        0, new AppModelBinderProvider());
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