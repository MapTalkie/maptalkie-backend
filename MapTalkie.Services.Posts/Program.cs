using System;
using MapTalkie.DB.Context;
using MapTalkie.Services.Posts.Consumers.PostCreatedConsumer;
using MapTalkie.Services.Posts.Consumers.PostLikedConsumer;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MapTalkie.Services.Posts
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", true);
                    config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true);
                    config.AddEnvironmentVariables();
                    config.AddCommandLine(args);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseNpgsql(context.Configuration.GetConnectionString("Postgres"),
                            x => x.UseNetTopologySuite());
                    });

                    var schedulerEndpoint = new Uri("queue:scheduler");

                    services.AddMassTransit(x =>
                    {
                        x.AddConsumer<PostLikedConsumer>(typeof(PostLikedConsumerDefinition));
                        x.AddConsumer<PostCreatedConsumer>(typeof(PostCreatedConsumerDefinition));
                        x.AddMessageScheduler(schedulerEndpoint);

                        x.UsingRabbitMq((busContext, cfg) =>
                        {
                            cfg.UseMessageScheduler(schedulerEndpoint);
                            cfg.ConfigureEndpoints(busContext);
                        });
                    });
                    services.AddHostedService<MassTransitConsoleHostedService>();
                });


            var host = builder.Build();
            host.Run();
        }
    }
}