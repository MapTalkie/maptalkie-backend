using MapTalkie.DB.Context;
using MapTalkie.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

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
            services.ConfigureAll(Configuration);
            services.AddAppControllers();
            services.AddAppMassTransit(Configuration);
            services.AddAppServices();
            services.AddAppCors(Configuration);
            services.AddAppDbContext(Configuration);
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
            else if (Configuration.GetValue("UseHsts", true))
                app.UseHsts();

            if (Configuration.GetValue("UseHttpsRedirection", true))
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

            // TODO как-то это исправить
            context.Database.Migrate();
            context.Database.OpenConnection();
            ((NpgsqlConnection)context.Database.GetDbConnection()).ReloadTypes();
        }
    }
}