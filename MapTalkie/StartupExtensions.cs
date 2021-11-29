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
using MapTalkie.Services.MessageService;
using MapTalkie.Services.PopularityProvider;
using MapTalkie.Services.TokenService;
using MapTalkie.Utils.Binders;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.RabbitMqTransport;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
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
                .AddScoped<IAuthService, AuthService>()
                .AddScoped<IPopularityProvider, PopularityProvider>()
                .AddScoped<IFriendshipService, FriendshipService>()
                .AddScoped<ITokenService, TokenService>()
                .AddScoped<IMessageService, MessageService>();
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
                    {
                        builder.WithOrigins(
                            "http://localhost:3000", "https://localhost:3000",
                            "http://localhost:5500", "https://localhost:5500",
                            "http://127.0.0.1:3000", "https://127.0.0.1:3000",
                            "http://127.0.0.1:5500", "https://127.0.0.1:5500"
                        );
                    }
                    else if (env.IsProduction())
                    {
                        // TODO
                        throw new NotImplementedException();
                    }

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
            // TODO поменять
            services.AddSingleton(new IdGenerator(0));
            services.AddDbContext<AppDbContext>(options =>
            {
                string connectionString = connectionStringProvider();
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
            converters.Add(new LatLonPointJsonConverter());
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
            services.AddIdentity<User, Role>()
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

        public static void InitApp(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            Action<AppInitializationConfiguration>? configurationFunction = null)
        {
            var cfg = new AppInitializationConfiguration();
            if (configurationFunction != null)
                configurationFunction(cfg);

            services.ConfigureAll(configuration);
            services.AddAppServices();

            if (cfg.ConnectionStringFactory != null)
                services.AddAppDbContext(cfg.ConnectionStringFactory);
            else
                services.AddAppDbContext(configuration);
            services.AddAppIdentity();

            if (cfg.UseControllers)
                services.AddAppControllers();
            if (cfg.UseSignalR)
                services.AddAppSignalR();
            if (cfg.UseCors)
                services.AddAppCors(environment);

            services.AddMemoryCache();

            services.AddAppAuthorization(
                configuration.GetSection<JwtSettings>(),
                configuration.GetSection<AuthenticationSettings>());
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