using System;
using System.IO;
using MapTalkie.DB.Context;
using MapTalkie.Hubs;
using MapTalkie.Services.Posts.Consumers.PostCreatedConsumer;
using MapTalkie.Services.Posts.Consumers.PostLikedConsumer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace MapTalkie.Tests.Integration.Fixtures
{
    public class TestStartup
    {
        private readonly string _databaseConnectionString = DBContstants.DatabaseConnectionString(
            $"maptalkie_integration_tests_{Guid.NewGuid():N}");

        public TestStartup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        private IConfiguration Configuration { get; }
        private IWebHostEnvironment Env { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            string a = Directory.GetCurrentDirectory();
            services.AddMvcCore().AddApplicationPart(typeof(Startup).Assembly);

            services.ConfigureAll(Configuration);
            services.AddAppControllers();
            services.AddAppMassTransit(Configuration, options =>
            {
                options.UseInMemory = true;
                options.ConfigureMassTransitBus = x =>
                {
                    x.AddConsumer<PostLikedConsumer>(typeof(PostLikedConsumerDefinition));
                    x.AddConsumer<PostCreatedConsumer>(typeof(PostCreatedConsumerDefinition));
                };
            });
            services.AddAppServices();
            services.AddAppCors(Env);
            services.AddAppDbContext(() => _databaseConnectionString);
            services.AddAppSignalR();
            // services.AddAppQuartz();
            services.AddMemoryCache();
            services.AddAppIdentity();
            services.AddAppAuthorization(Configuration);
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
                endpoints.MapHub<UserHub>("/_signalr/user");
            });

            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.Migrate();
            context.Database.OpenConnection();
            ((NpgsqlConnection)context.Database.GetDbConnection()).ReloadTypes();
        }
    }

    public class NpgsqlConnect
    {
    }
}