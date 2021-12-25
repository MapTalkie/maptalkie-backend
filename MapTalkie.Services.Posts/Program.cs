using System;
using MapTalkie.DB.Context;
using MapTalkie.Services.Posts.Consumers.PostCreatedConsumer;
using MapTalkie.Services.Posts.Consumers.PostLikedConsumer;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MapTalkie.Services.Posts;

internal class Program
{
    private static void Main(string[] args)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Development;
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{env}.json")
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        var builder = new HostBuilder()
            .ConfigureLogging(logging =>
            {
                logging.AddConfiguration(configuration.GetSection("Logging"));
                logging.AddConsole();
            })
            .ConfigureAppConfiguration((context, cfg) =>
            {
                context.HostingEnvironment.EnvironmentName = env;
                cfg.Sources.Clear();
                cfg.AddConfiguration(configuration);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddLogging();
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseNpgsql(context.Configuration.GetConnectionString("Postgres"),
                        x => x.UseNetTopologySuite());
                });

                services.AddMassTransit(x =>
                {
                    x.AddConsumer<PostLikedConsumer>(typeof(PostLikedConsumerDefinition));
                    x.AddConsumer<PostCreatedConsumer>(typeof(PostCreatedConsumerDefinition));

                    x.UsingRabbitMq((busContext, cfg) =>
                    {
                        cfg.ConfigureEndpoints(busContext);
                        cfg.Host(configuration.GetSection("RabbitMQ").GetValue<string>("Host"));
                    });
                });
                services.AddHostedService<MassTransitConsoleHostedService>();
            });


        var host = builder.Build();
        host.Run();
    }
}