using MapTalkie.Configuration;
using MapTalkie.Consumers;
using MapTalkie.DB;
using MapTalkie.DB.Context;
using MapTalkie.Hubs;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MapTalkie
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        private IConfiguration Configuration { get; }
        private IWebHostEnvironment Env { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMassTransit(options =>
            {
                options.AddConsumer<UserRelatedEventsConsumer>();
                options.UsingRabbitMq((context, cfg) =>
                {
                    var config = Configuration.GetSection("RabbitMQ")?.Get<RabbitMQConfiguration>();
                    if (config != null)
                    {
                        cfg.Host(config.Host, h =>
                        {
                            if (config.Username != null)
                                h.Username(config.Username);

                            if (config.Password != null)
                                h.Username(config.Password);
                        });
                    }
                });
            });
            services.AddMassTransitHostedService();

            services.AddAppControllers();
            services.ConfigureAll(Configuration);
            services.AddAppServices();
            services.AddAppCors(Env);
            services.AddAppDbContext(Configuration);
            services.AddAppSignalR();
            services.AddAppQuartz();
            services.AddMemoryCache();

            services
                .AddIdentity<User, Role>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.AddAppAuthorization(
                Configuration.GetSection<JwtSettings>(),
                Configuration.GetSection<AuthenticationSettings>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseHealthChecks("/health");

            // на будущее: UseCors должно стоять до UseAuthorization, чтобы ASP.NET не пытался 
            // авторизовать OPTIONS запросы
            app.UseCors();

            app.UseAuthorization();

            app.UseWebSockets();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<MainUserHub>("/_signalr/main");
            });

            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();
        }
    }
}