using System;
using System.Collections.Generic;
using IdGen;
using MapTalkie.Configuration;
using MapTalkie.Consumers;
using MapTalkie.DB;
using MapTalkie.DB.Context;
using MapTalkie.Domain.Utils.JsonConverters;
using MapTalkie.Services.AuthService;
using MapTalkie.Services.FriendshipService;
using MapTalkie.Services.TokenService;
using MapTalkie.Utils.Binders;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.RabbitMqTransport;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                .AddScoped<IAuthService, AuthService>()
                .AddScoped<IFriendshipService, FriendshipService>()
                .AddScoped<ITokenService, TokenService>();
        }

        public static void ConfigureAll(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
            services.Configure<AuthenticationSettings>(configuration.GetSection(nameof(AuthenticationSettings)));
        }

        public static void AddAppCors(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = configuration.GetSection<CorsSettings>();
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins(settings.Origins);
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

        public static void AddAppDbContext(
            this IServiceCollection services, IConfiguration configuration)
            => services.AddAppDbContext(() => configuration.GetConnectionString("Postgres"));

        public static void AddAppDbContext(
            this IServiceCollection services,
            Func<string> connectionStringProvider)
        {
            // добавляем
            services.AddScoped(_ => new IdGenerator(Environment.CurrentManagedThreadId));
            services.AddDbContext<AppDbContext>(options =>
            {
                string connectionString = connectionStringProvider();
                options.UseNpgsql(connectionString,
                    builder =>
                    {
                        builder.UseNetTopologySuite();
                        builder.MigrationsAssembly(typeof(Startup).Assembly.GetName().Name);
                    });
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
            converters.Add(new LatLonPointJsonConverter());
            converters.Add(new IdToStringConverter());
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

        public static void AddAppIdentity(this IServiceCollection services)
        {
            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
        }

        public static void AddAppMassTransit(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<MassTransitAppOptions>? optionsFunction = null)
        {
            var cfg = new MassTransitAppOptions();
            if (optionsFunction != null)
                optionsFunction(cfg);

            services.AddMassTransit(options =>
            {
                options.AddConsumer<UserRelatedEventsConsumer>();

                cfg.ConfigureMassTransitBus?.Invoke(options);

                if (cfg.UseInMemory)
                {
                    options.UsingInMemory((context, cfg) =>
                    {
                        cfg.TransportConcurrencyLimit = 100;

                        cfg.ConfigureEndpoints(context);
                    });
                }
                else if (cfg.ConfigureRabbitMq == null)
                {
                    options.UsingRabbitMq((context, cfg) =>
                    {
                        var config = configuration.GetSection("RabbitMQ")?.Get<RabbitMQConfiguration>();
                        if (config != null)
                            cfg.Host(config.Host, h =>
                            {
                                if (config.Username != null)
                                    h.Username(config.Username);

                                if (config.Password != null)
                                    h.Username(config.Password);
                            });
                    });
                }
                else
                {
                    options.UsingRabbitMq(cfg.ConfigureRabbitMq);
                }
            });
            services.AddMassTransitHostedService();
        }

        public class MassTransitAppOptions
        {
            public bool UseInMemory { get; set; }
            public Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>? ConfigureRabbitMq { get; set; }
            public Action<IServiceCollectionBusConfigurator>? ConfigureMassTransitBus { get; set; }
        }
    }

    public class AppInitializationConfiguration
    {
        public bool UseSignalR { get; set; } = true;
        public bool UseControllers { get; set; } = true;
        public bool UseCors { get; set; } = true;
        public bool UseInMemoryMassTransit { get; set; } = false;
        public Func<string>? ConnectionStringFactory { get; set; }
        public Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>? ConfigureRabbitMq { get; set; }
        public Action<IServiceCollectionBusConfigurator>? ConfigureMassTransitBus { get; set; }
    }
}