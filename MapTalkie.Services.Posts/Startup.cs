using MapTalkie.Services.Posts.Models;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MapTalkie.Services.Posts
{
    internal class Startup
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
            services.AddDbContext<PostsDbContext>(options =>
            {
                options.UseNpgsql(
                    Configuration.GetConnectionString("Postgres"),
                    x => x.UseNetTopologySuite());
            });

            var rabbitMqHost = Configuration.GetSection("RabbitMQ")["Host"];

            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((busContext, cfg) =>
                {
                    cfg.Host(rabbitMqHost);
                    cfg.ConfigureEndpoints(busContext);
                });
            });
            services.AddHostedService<MassTransitConsoleHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseHealthChecks("/health");
        }
    }
}